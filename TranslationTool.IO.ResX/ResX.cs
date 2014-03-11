using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Linq;
using System;

namespace TranslationTool.IO
{
	public class ResXTranslationProject : ITranslationProject
	{
		TranslationProject Collection;
		string Directory;
		string MasterLanguage;

		public ResXTranslationProject(string directory, string masterLanguage)
		{
			this.Collection = new TranslationProject();
			this.Directory = directory;

			this.ModuleNames = IO.Collection.ResX.GetModuleNames(directory);
			this.MasterLanguage = masterLanguage;
		}

		public TranslationModule this[string moduleName]
		{
			get
			{
				if (!Collection.Projects.ContainsKey(moduleName) && ModuleNames.Contains(moduleName))
				{
					Collection.Projects.Add(moduleName, IO.ResX.FromResX(this.Directory, moduleName, this.MasterLanguage));
				}


				return Collection.Projects[moduleName];
			}

			set
			{
				if (!Collection.Projects.ContainsKey(moduleName))
				{
					Collection.Projects.Add(moduleName, value);
					IO.ResX.ToResX(value, this.Directory);
				}
			}
		}

		public IEnumerable<string> ModuleNames { get; set; }
		public IEnumerable<TranslationModule> Modules
		{
			get
			{
				return ModuleNames.Select(name => this[name]);
			}
		}

		public void Add(TranslationModule module)
		{
			throw new NotImplementedException();
		}

		public void SyncWith(ITranslationProject other)
		{
			throw new NotImplementedException();
		}
	}


	public class ResX
	{
		public static TranslationModule FromResX(string dir, string project, string masterLanguage)
		{
			var tp = new TranslationModule(project, masterLanguage);
			List<DateTime> fileDates = new List<DateTime>();

			var masterFile = dir + project + ".resx";
			tp.Add(Segment.FromDict(masterLanguage, GetDictFromResX(masterFile)));
			fileDates.Add(System.IO.File.GetLastWriteTimeUtc(masterFile));

			foreach (var l in tp.Languages)
			{
				if (l == masterLanguage) continue; //we skip master language since we treated it already as a special case

				var file = dir + project + "." + l + ".resx";
				tp.Add(Segment.FromDict(l, GetDictFromResX(file)));
				fileDates.Add(System.IO.File.GetLastWriteTimeUtc(file));
			}

			tp.LastModified = fileDates.Max();
			return tp;
		}

		public static IEnumerable<string> GetResXFileNames(string directory_, string name, IEnumerable<string> languages)
		{
			var directory = directory_ + @"\";
			return languages.Select(l => directory + name + "." + l + ".resx").Union(new []{directory + name + ".resx"});
		}

		static Dictionary<string, string> GetDictFromResX(string fileName)
		{
			// Enumerate the resources in the file.
			//ResXResourceReader rr = ResXResourceReader.FromFileContents(file);
			var stringDict = new Dictionary<string, string>();

			if (!System.IO.File.Exists(fileName))
				return stringDict;

			ResXResourceReader rr = new ResXResourceReader(fileName);
			IDictionaryEnumerator dict = rr.GetEnumerator();
			
			while (dict.MoveNext())
				stringDict.Add(dict.Key as string, dict.Value as string);

			return stringDict;
		}

		public static void ToResX(TranslationModule tp, string targetDir, string moduleName = null)
		{
			var byLanguage = tp.ByLanguage;
			moduleName = moduleName ?? tp.Name;

			ToResX(byLanguage[tp.MasterLanguage], targetDir + @"\" + moduleName + ".resx");
			foreach (var l in tp.Languages)
			{
				if (l == tp.MasterLanguage) continue; //we skip master language since we treated it already as a special case

				IEnumerable<Segment> segments;
				if (byLanguage.Contains(l))
					segments = byLanguage[l];
				else
					segments = Segment.EmptyFromTemplate(byLanguage[tp.MasterLanguage]);

				if (segments.Count() > 0) 
				{
					ToResX(segments, targetDir + @"\" + moduleName + "." + l + ".resx");
				}
			}
		}

		protected static void ToResX(Dictionary<string, string> dict, string fileName)
		{
			using (ResXResourceWriter resx = new ResXResourceWriter(fileName))
			{
				foreach (var kvp in dict)
					resx.AddResource(kvp.Key, kvp.Value);
			}
		}
		protected static void ToResX(IEnumerable<Segment> segments, string fileName)
		{
			segments = segments.Where(s => !string.IsNullOrWhiteSpace(s.Text));
			using (ResXResourceWriter resx = new ResXResourceWriter(fileName))
			{
				foreach (var kvp in segments)
					resx.AddResource(kvp.Key, kvp.Text);
			}
		}
	}
}
