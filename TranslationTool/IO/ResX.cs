using System.Collections;
using System.Collections.Generic;
using System.Resources;

namespace TranslationTool.IO
{
	public class ResX
	{
		public static TranslationProject FromResX(string dir, string project)
		{
			var tp = new TranslationProject(project);

			tp.masterDict = GetDictFromResX(dir + project + ".resx");

			foreach (var l in tp.Languages)
			{
				var file = dir + project + "." + l + ".resx";
				tp.dicts.Add(l, GetDictFromResX(file));
			}
			return tp;
		}

		static Dictionary<string, string> GetDictFromResX(string fileName)
		{
			// Enumerate the resources in the file.
			//ResXResourceReader rr = ResXResourceReader.FromFileContents(file);
			ResXResourceReader rr = new ResXResourceReader(fileName);
			IDictionaryEnumerator dict = rr.GetEnumerator();

			var stringDict = new Dictionary<string, string>();
			while (dict.MoveNext())
				stringDict.Add(dict.Key as string, dict.Value as string);

			return stringDict;
		}

		public static void ToResX(TranslationProject tp, string targetDir)
		{
			ToResX(tp.masterDict, targetDir + tp.project + ".resx");
			foreach (var l in tp.Languages)
			{
				Dictionary<string, string> dict;
				if (tp.dicts.ContainsKey(l))
					dict = tp.dicts[l];
				else
					dict = TranslationProject.EmptyFromTemplate(tp.masterDict);

				ToResX(dict, targetDir + tp.project + "." + l + ".resx");
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
	}
}
