using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslationTool.IO.Collection
{
	class Xls
	{
		public static IEnumerable<KeyValuePair<string, TranslationProject>> ToDir(TranslationProjectCollection tpc, string targetDir)
		{
			Dictionary<string, TranslationProject> fileNames = new Dictionary<string, TranslationProject>();
			foreach (var kvp in tpc.Projects)
			{
				var fileName = targetDir + @"\" + kvp.Key + ".xls";
				IO.Xls.ToXLS(kvp.Value, fileName);

				fileNames.Add(fileName, kvp.Value);
			}

			return fileNames;
		}
	}
}
