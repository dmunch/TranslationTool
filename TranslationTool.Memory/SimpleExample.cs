using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Similar;
using Lucene.Net.Store;

namespace TranslationTool.Memory
{
	class SimpleExample
	{
		public static void TestLucene()
		{
			var tmp = TranslationMemoryHelpers.TempPath;
			var dir = FSDirectory.Open(tmp);
			bool created;
			TranslationMemoryHelpers.IsIndexExists(out created, dir);


			IndexWriter writer = new IndexWriter(dir, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30), false, IndexWriter.MaxFieldLength.UNLIMITED);

			IndexDocument(writer, "About Hockey", "hockey", "Hockey is a cool sport which I really like, bla bla");
			IndexDocument(writer, "Some great players", "hockey", "Some of the great players from Sweden - well Peter Forsberg, Mats Sunding, Henrik Zetterberg");
			IndexDocument(writer, "Soccer info", "soccer", "Soccer might not be as fun as hockey but it's also pretty fun");
			IndexDocument(writer, "Players", "soccer", "From Sweden we have Zlatan Ibrahimovic and Henrik Larsson. They are the most well known soccer players");
			IndexDocument(writer, "1994", "soccer", "I remember World Cup 1994 when Sweden took the bronze. we had great players. players , bla bla");

			writer.Optimize();
			writer.Close();

			Search(dir);
		}

		private static void IndexDocument(IndexWriter writer, string sHeader, string sType, string sContent)
		{
			Document doc = new Document();

			doc.Add(new Field("header", sHeader, Field.Store.YES, Field.Index.ANALYZED));
			doc.Add(new Field("type", sType, Field.Store.YES, Field.Index.NOT_ANALYZED));
			doc.Add(new Field("content", sContent, Field.Store.YES, Field.Index.ANALYZED));

			writer.AddDocument(doc);

		}

		private static void Search(Directory path)
		{
			var ir = IndexReader.Open(path, true);
			var mlt = new MoreLikeThis(ir);
			mlt.SetFieldNames(new string[] { "content" });
			mlt.MinTermFreq = 1;
			mlt.MinDocFreq = 1;

			var reader = new System.IO.StringReader("are the most well known");
			var query = mlt.Like(reader);

			using (var searcher = new IndexSearcher(path, true))
			{
				var topDocs = searcher.Search(query, 5);
				foreach (var scoreDoc in topDocs.ScoreDocs)
				{
					Document doc = searcher.Doc(scoreDoc.Doc);
				}
			}
		}
	}
}
