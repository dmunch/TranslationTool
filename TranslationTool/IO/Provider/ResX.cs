using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Linq;

namespace TranslationTool.IO
{
	public class ResX
	{
		public static TranslationModule FromResX(string dir, string project, string masterLanguage)
		{
			var tp = new TranslationModule(project, masterLanguage);

			tp.Add(Segment.FromDict(masterLanguage, GetDictFromResX(dir + project + ".resx")));

			foreach (var l in tp.Languages)
			{
				if (l == masterLanguage) continue; //we skip master language since we treated it already as a special case

				var file = dir + project + "." + l + ".resx";
				tp.Add(Segment.FromDict(l, GetDictFromResX(file)));
			}
			
			return tp;
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

		public static void ToResX(TranslationModule tp, string targetDir)
		{
			var byLanguage = tp.ByLanguage;
			ToResX(byLanguage[tp.MasterLanguage], targetDir + tp.Name + ".resx");
			foreach (var l in tp.Languages)
			{
				if (l == tp.MasterLanguage) continue; //we skip master language since we treated it already as a special case

				IEnumerable<Segment> segments;
				if (byLanguage.Contains(l))
					segments = byLanguage[l];
				else
					segments = Segment.EmptyFromTemplate(byLanguage[tp.MasterLanguage]);

				ToResX(segments, targetDir + tp.Name + "." + l + ".resx");
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
