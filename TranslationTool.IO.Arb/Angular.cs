﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace TranslationTool.IO
{
	public class Angular
	{
		public static void ToAngular(TranslationModule tp, string targetDir)
		{
			StringBuilder sb = new StringBuilder();
			
			sb.AppendLine("app.config(function($translateProvider) {");

			foreach (var language in tp.ByLanguage)
			{
				sb.AppendLine(string.Format(" $translateProvider.translations('{0}', {{", language.Key));
				foreach (var segment in language.Where(l => !string.IsNullOrWhiteSpace(l.Text)))
				{
					sb.Append("\"").Append(segment.Key).Append("\":\"").Append(segment.Text).AppendLine("\",");
				}
				sb.AppendLine("},");
			}
			sb.AppendLine("}");

			using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + tp.Name + ".lang.js", false, Encoding.UTF8))
			{
				outfile.Write(sb.ToString());
			}
		}
	}
}
