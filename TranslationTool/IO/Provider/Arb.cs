using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TranslationTool.IO
{
	public class Arb
	{
		public static void ToArb(TranslationModule tp, string targetDir)
		{
			StringBuilder sb = new StringBuilder();

			foreach (var l in tp.ByLanguage)
			{
				sb = ToArb(sb, tp.Name, l.Key, l);
			}

			/*
			 using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + project + ".arb", false, Encoding.UTF8))
			{
				outfile.Write(sb.ToString());
			}*/
		}

		public static void ToArbAll(TranslationModule tp, string targetDir)
		{
			StringBuilder sb = new StringBuilder();

			foreach (var l in tp.ByLanguage)
			{ 
					sb = ToArb(sb, tp.Name, l.Key, l);
			}

			using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + tp.Name + ".arb", false, Encoding.UTF8))
			{
				outfile.Write(sb.ToString());
			}
		}

		public static StringBuilder ToArb(string targetDir, string project, string language, IEnumerable<Segment> segments)
		{
			StringBuilder sb = new StringBuilder();

			sb = ToArb(sb, project, language, segments);
			using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + project + "." + language + ".arb", false, Encoding.UTF8))
			{
				outfile.Write(sb.ToString());
			}

			return sb;
		}

		public static StringBuilder ToArb(StringBuilder sb, string project, string language, IEnumerable<Segment> segments)
		{
			string newLine = "";
			sb.Append("arb.register(\"arb_ref_app\",{").Append(newLine);
			sb.Append("\"@@locale\":\"").Append(language).Append("\",").Append(newLine);
			sb.Append("\"@@context\":\"").Append(project).Append("\",").Append(newLine);

			foreach (var kvp in segments)
			{
				sb.Append("\"").Append(kvp.Key).Append("\":\"").Append(kvp.Text).Append("\",").Append(newLine);
			}
			sb.Remove(sb.Length - 1, 1).Append(newLine); //remove trailing ,
			sb.Append("});").Append(newLine).Append(newLine);

			return sb;
		}
	}
}
