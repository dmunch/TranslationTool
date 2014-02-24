using System;
using System.Linq.Expressions;
using System.IO;

namespace TranslationTool.Helpers
{
	public class StringContentDiff<T, TKey> : GeneralDiff<T, TKey, string>
	{
		public StringContentDiff(Expression<Func<T, TKey>> KeySelector, Expression<Func<T, string>> contentSelector)
			: base(KeySelector, contentSelector)
		{
			this.IsModifiedComparer = (s1, s2) => s1 != s2; //&& !string.IsNullOrWhiteSpace(s1) && !string.IsNullOrWhiteSpace(s2);
		}

		/*
		public void PrintDiff(TextWriter os = null)
		{
			if (os == null)
				os = Console.Out;

			var diff = new DiffMatchPatch.diff_match_patch();
			foreach (var kvp in Updated)
			{
				var diffs = diff.diff_main(ContentSelector(Orig[kvp.Key]), ContentSelector(kvp.Value));
				diff.diff_cleanupSemantic(diffs);
				os.WriteLine("K: {0}, diffs {1}", kvp.Key, diffs.Count);

				foreach (var d in diffs)
				{
					os.WriteLine("{0}: {1}", d.operation, d.text);
				}
				//diff.diff_prettyHtml(diffs);
			}
		}
		*/
		
	}
}
