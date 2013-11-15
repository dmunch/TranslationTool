using System.Collections.Generic;
using System.IO;
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
							for (int i = 1; i < fieldCount; i++)
							{
								if (i != commentColumn)
								{
									tp.Dicts[headers[i].ToLower()].Add(key, csv[i]);
								}
								else
								{
									tp.Comments.Add(key, csv[i]);
								}
							}
					}
				}
			}

			return tp;
		}

		public static void ToCSV(TranslationModule project, string targetDir)
		{
			StringBuilder sb = new StringBuilder();
			ToCSV(project, sb, true);

			using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + project.Project + ".csv", false, Encoding.UTF8))
			{
				outfile.Write(sb.ToString());
			}
		}

		public static void ToCSV(TranslationModule project, StringBuilder sb, bool addHeader = true)
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
			foreach (var key in project.Keys)
			{
				sb.Append(key).Append(";");
				foreach (var l in project.Languages)
				{
					sb.Append("'");
					sb.Append(project.Dicts[l].ContainsKey(key) ? project.Dicts[l][key] : "");
					sb.Append("';");
				}

				sb.AppendLine();
			}
		}
	}
}
