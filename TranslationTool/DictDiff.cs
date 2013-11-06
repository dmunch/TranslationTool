using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TranslationTool
{
	public class DictDiff
	{
		public Dictionary<string, string> Orig = new Dictionary<string, string>();
		public Dictionary<string, string> Updated = new Dictionary<string, string>();
		public Dictionary<string, string> New = new Dictionary<string, string>();

		public DictDiff()
		{
		}

		public void PrintDiff(TextWriter os = null)
		{
			if (os == null)
				os = Console.Out;
		
			var diff = new DiffMatchPatch.diff_match_patch();
			foreach (var kvp in Updated)
			{
				var diffs = diff.diff_main(Orig[kvp.Key], kvp.Value);
				diff.diff_cleanupSemantic(diffs);
				os.WriteLine("K: {0}, diffs {1}", kvp.Key, diffs.Count);

				foreach (var d in diffs)
				{
					os.WriteLine("{0}: {1}", d.operation, d.text);
				}
				//diff.diff_prettyHtml(diffs);
			}
		}

		public void Print(string language, TextWriter os = null)
		{
			if (os == null)
				os = Console.Out;

			os.WriteLine("Updated {0} rows in {1}.", Updated.Count, language);
			foreach (var kvp in Updated)
			{ 
				os.WriteLine("K: {0}\n Old: {1} \n New: {2}", kvp.Key, Orig[kvp.Key], kvp.Value);
			}

			os.WriteLine("Added {0} rows in {1}.", New.Count, language);
			foreach (var kvp in New)
			{ 
				os.WriteLine(kvp.Key);
			}
		}

		public static DictDiff SyncDicts(Dictionary<string, string> d1, Dictionary<string, string> d2)
		{
			var diff = Diff(d1, d2);
			Patch(d1, diff);

			return diff;
		}

		public static void Patch(Dictionary<string, string> d, DictDiff toSync)
		{
			foreach (var kvp in toSync.New)
			{
				if (!d.ContainsKey(kvp.Key))
				{
					d.Add(kvp.Key, kvp.Value);
				}
			}

			foreach (var kvp in toSync.Updated)
			{
				d[kvp.Key] = kvp.Value;
			}
		}


		public static DictDiff Diff(Dictionary<string, string> d1, Dictionary<string, string> d2)
		{
			var toSync = new DictDiff();

			foreach (var kvp in d1)
			{
				if (d2.ContainsKey(kvp.Key) && kvp.Value != d2[kvp.Key] && d2[kvp.Key].Trim() != "")
				{
					toSync.Updated.Add(kvp.Key, d2[kvp.Key]);
					toSync.Orig.Add(kvp.Key, kvp.Value);
				}
			}

			//don't add new keys            
			foreach (var kvp in d2)
			{
				if (!d1.ContainsKey(kvp.Key))
				{
					toSync.New.Add(kvp.Key, kvp.Value);
				}
			}

			return toSync;
		}
	}
}
