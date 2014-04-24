using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LumenWorks.Framework.IO.Csv;
using System;

namespace TranslationTool.IO
{
	public class CSV
	{
		public static TranslationModule FromCSV(string file, string project, string masterLanguage, bool createMissingKeys = true)
		{
			// open the file "data.csv" which is a CSV file with headers
			TranslationModule tp = null;

			List<string> languages = new List<string>();

			using (CsvReader csv =
				   new CsvReader(new StreamReader(file), true))
			{
				int fieldCount = csv.FieldCount;
				string currentNS = "";
				int commentColumn = -1;

				string[] headers = csv.GetFieldHeaders();

				for (int c = 1; c < headers.Length; c++)
				{
					string language = headers[c].ToLower();
					if (language == "Comment")
					{
						commentColumn = c;
						continue;
					}

					languages.Add(language);
				}
				tp = new TranslationModule(project, masterLanguage, languages.ToArray());

				while (csv.ReadNextRecord())
				{
					string key = csv[0];
					if (key.Contains("ns:"))
						currentNS = key.Split(':')[1];

					if (currentNS == project && !key.Contains("ns:"))
					{
						if (string.IsNullOrWhiteSpace(key) && createMissingKeys)
						{
							string keyInspiration = commentColumn != 1 ? csv[1] : csv[2];
							key = tp.KeyProposal(keyInspiration);
						}

						if (!string.IsNullOrWhiteSpace(key))
						{
							for (int i = 1; i < fieldCount; i++)
							{
								if (i != commentColumn)
								{
									//tp.Dicts[headers[i].ToLower()].Add(key, csv[i]);
									tp.Add(new Segment(headers[i].ToLower(), key, csv[i]));
								}
							}
							if (commentColumn != -1)
							{ 
								foreach(var seg in tp.ByKey[key])								
								{
									seg.Comment = csv[commentColumn];
								}
							}
						}
							
							
					}
				}
			}

			return tp;
		}

		public static void ToCSV(ITranslationModule project, string targetDir)
		{
			StringBuilder sb = new StringBuilder();
			ToCSV(project, sb, true);

			using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + project.Name + ".csv", false, Encoding.UTF8))
			{
				outfile.Write(sb.ToString());
			}
		}

		public static void ToCSV(ITranslationModule project, StringBuilder sb, bool addHeader = true)
		{
			if (addHeader)
			{
				sb.Append("").Append("en").Append(";");
				foreach (var l in project.Languages)
				{
					sb.Append(l);
					sb.Append(";");
				}
				sb.AppendLine();
			}

			var byKey = project.ByKey;
			foreach (var key in project.Keys)
			{
				sb.Append(key).Append(";");
				foreach (var l in project.Languages)
				{
					sb.Append("'");
					var seg = byKey[key].FirstOrDefault(s => s.Language == l);
					sb.Append(seg != null ? seg.Text : "");
					sb.Append("';");
				}

				sb.AppendLine();
			}
		}
	}
}
