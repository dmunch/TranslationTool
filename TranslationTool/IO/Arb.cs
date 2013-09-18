using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TranslationTool.IO
{
	public class Arb
	{
		public static void ToArb(TranslationProject tp, string targetDir)
		{
			StringBuilder sb = new StringBuilder();

			sb = ToArb(targetDir, tp.project, tp.masterLanguage, tp.masterDict);
			foreach (var l in tp.Languages)
				if (tp.dicts.ContainsKey(l))
					sb = ToArb(targetDir, tp.project, l, tp.dicts[l]);
			/*
			 using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + project + ".arb", false, Encoding.UTF8))
			{
				outfile.Write(sb.ToString());
			}*/
		}

		public static void ToArbAll(TranslationProject tp, string targetDir)
		{
			StringBuilder sb = new StringBuilder();

			sb = ToArb(sb, tp.project, tp.masterLanguage, tp.masterDict);
			foreach (var l in tp.Languages)
				if (tp.dicts.ContainsKey(l))
					sb = ToArb(sb, tp.project, l, tp.dicts[l]);

			using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + tp.project + ".arb", false, Encoding.UTF8))
			{
				outfile.Write(sb.ToString());
			}
		}

		public static StringBuilder ToArb(string targetDir, string project, string language, Dictionary<string, string> dict)
		{
			StringBuilder sb = new StringBuilder();

			sb = ToArb(sb, project, language, dict);
			using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + project + "." + language + ".arb", false, Encoding.UTF8))
			{
				outfile.Write(sb.ToString());
			}

			return sb;
		}

		public static StringBuilder ToArb(StringBuilder sb, string project, string language, Dictionary<string, string> dict)
		{
			string newLine = "";
			sb.Append("arb.register(\"arb_ref_app\",{").Append(newLine);
			sb.Append("\"@@locale\":\"").Append(language).Append("\",").Append(newLine);
			sb.Append("\"@@context\":\"").Append(project).Append("\",").Append(newLine);

			foreach (var kvp in dict)
			{
				sb.Append("\"").Append(kvp.Key).Append("\":\"").Append(kvp.Value).Append("\",").Append(newLine);
			}
			sb.Remove(sb.Length - 1, 1).Append(newLine); //remove trailing ,
			sb.Append("});").Append(newLine).Append(newLine);

			return sb;
		}
	}
}
