using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TranslationTool.IO.Collection
{
	public class CSV
	{
		public static TranslationProject FromCSV(IEnumerable<string> projectNames, string fileName, string masterLanguage)
		{
			var tpc = new TranslationProject();


			foreach (var pName in tpc.ModuleNames)
				tpc.Projects.Add(pName, IO.CSV.FromCSV(fileName, pName, masterLanguage));

			return tpc;
		}

		public void ToCSV(TranslationProject tpc, string targetDir)
		{
			StringBuilder sb = new StringBuilder();
			string fileName = "";
			foreach (var kvp in tpc.Projects)
			{
				sb.Append("ns:").AppendLine(kvp.Key);
				IO.CSV.ToCSV(kvp.Value, sb, false);

				fileName += "_" + kvp.Key;
			}

			using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + fileName + ".csv", false, Encoding.UTF8))
				outfile.Write(sb.ToString());
		}

	}
}
