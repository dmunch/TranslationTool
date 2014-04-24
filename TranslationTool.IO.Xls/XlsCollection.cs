using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslationTool.IO.Collection
{
	public class XlsCollection
	{
		public static IEnumerable<KeyValuePair<string, ITranslationModule>> ToDir(ITranslationProject tpc, string targetDir)
		{
			Dictionary<string, ITranslationModule> fileNames = new Dictionary<string, ITranslationModule>();
			foreach (var moduleName in tpc.ModuleNames)
			{
				var module = tpc[moduleName];

				var fileName = targetDir + @"\" + moduleName + ".xls";
				IO.Xls.ToXLS(module, fileName);

				fileNames.Add(fileName, module);
			}

			return fileNames;
		}
	}
}
