using System;
using System.Linq;

using System.Collections.Generic;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Similar;
using Lucene.Net.Store;

namespace TranslationTool.Memory
{	
	public class QueryResult : SegmentSet
	{
		public float Score { get; protected set; }
		public SegmentSet Segments { get; protected set; }

		public QueryResult(string key, IEnumerable<Segment> segments, float score) : base(key, segments)
		{
			this.Score = score;		
		}
	}

	public class TranslationMemory
	{
		protected Lucene.Net.Store.Directory Index;
		protected TranslationProject TranslationProject;

		public TranslationMemory(TranslationProject tp)
		{
			this.Index = TranslationMemoryHelpers.BuildTranslationProjectIndex(new TranslationProject[] {tp});
			this.TranslationProject = tp;
		}

		public TranslationMemory(TranslationProjectCollection tpCollection)
		{
			Index = TranslationMemoryHelpers.BuildTranslationProjectIndex(tpCollection.Projects.Values);
			this.TranslationProject = tpCollection.Projects.First().Value;
		}

		public IEnumerable<QueryResult> Query(string language, string query)
		{
			return TranslationMemoryHelpers.SearchTranslationProjects(Index, language, query, TranslationProject.Languages);
		}
	}

	public class TranslationMemoryHelpers
	{
		public static string TempPath
		{
			get
			{
				var tmpPath = System.IO.Path.GetTempPath() + @"\lucenetest\";
				try
				{
					System.IO.Directory.Delete(tmpPath, true);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
				System.IO.Directory.CreateDirectory(tmpPath);


				return tmpPath;
			}
		}
				
		public static Lucene.Net.Store.Directory BuildTranslationProjectIndex(IEnumerable<TranslationProject> tps)
		{
			var tmp = TempPath;
			var dir = Lucene.Net.Store.FSDirectory.Open(tmp);
			bool created;
			IsIndexExists(out created, dir);

			using (IndexWriter writer = new IndexWriter(dir, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30), false, IndexWriter.MaxFieldLength.UNLIMITED))
			{ 
				foreach(var tp in tps)
					IndexDocument(writer, tp);

				writer.Optimize();
			}
			
			return dir;			
		}

		public static IEnumerable<QueryResult> SearchTranslationProjects(Lucene.Net.Store.Directory dir, string lang, string searchText, IEnumerable<string> languages)
		{
			var ir = IndexReader.Open(dir, true);
			var mlt = new MoreLikeThis(ir);
			mlt.SetFieldNames(new string[] { lang });
			mlt.MinTermFreq = 1;
			mlt.MinDocFreq = 1;
			mlt.MinWordLen = 4;

			var reader = new System.IO.StringReader(searchText);
			var query = mlt.Like(reader);
			var results = new List<QueryResult>();

			using (var searcher = new IndexSearcher(dir, true))
			{
				var topDocs = searcher.Search(query, 1000);
				foreach (var scoreDoc in topDocs.ScoreDocs)
				{
					Document doc = searcher.Doc(scoreDoc.Doc);
					float score = scoreDoc.Score;
					
					var trads = languages.Select(l => new Segment(l, doc.Get(l)));
					var set = new SegmentSet(doc.Get("key"), trads);

					results.Add(new QueryResult(doc.Get("key"), trads, score));
				}
			}
			//var g = results.GroupBy(s => s);

			return results;
		}

		private static void IndexDocument(IndexWriter writer, TranslationProject tp)
		{
			foreach (var key in tp.Keys)
			{
				Document doc = new Document();

				doc.Add(new Field("key", key, Field.Store.YES, Field.Index.NOT_ANALYZED));

				foreach (var lang in tp.Languages)
				{ 
					if(tp.Dicts[lang].ContainsKey(key))
						doc.Add(new Field(lang, tp.Dicts[lang][key], Field.Store.YES, Field.Index.ANALYZED));
				}

				writer.AddDocument(doc);
			}
		}

		public static void IsIndexExists(out bool created, Directory dir)
		{			
			created = false;
			
			if (!IndexReader.IndexExists(dir))
			{
				IndexWriter writer = new IndexWriter(dir, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30), true, IndexWriter.MaxFieldLength.UNLIMITED);
				created = true;
				writer.Close();
			}
			
		}
	}
}
 