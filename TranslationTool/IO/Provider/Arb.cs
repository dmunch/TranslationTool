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

			foreach (var l in tp.Languages)
				if (tp.Dicts.ContainsKey(l))
					sb = ToArb(targetDir, tp.Project, l, tp.Dicts[l]);
			/*
			 using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + project + ".arb", false, Encoding.UTF8))
			{
				outfile.Write(sb.ToString());
			}*/
		}

		public static void ToArbAll(TranslationProject tp, string targetDir)
		{
			StringBuilder sb = new StringBuilder();
		
			foreach (var l in tp.Languages)
				if (tp.Dicts.ContainsKey(l))
					sb = ToArb(sb, tp.Project, l, tp.Dicts[l]);

			using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + tp.Project + ".arb", false, Encoding.UTF8))
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
