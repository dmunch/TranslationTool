using System.Collections.Generic;
using System.IO;
using System.Text;
using LumenWorks.Framework.IO.Csv;

namespace TranslationTool.IO
{
	public class CSV
	{
		public static TranslationProject FromCSV(string file, string project, string masterLanguage)
		{
			// open the file "data.csv" which is a CSV file with headers
			var dicts = new Dictionary<string, Dictionary<string, string>>();
			List<string> languages = new List<string>();

			using (CsvReader csv =
				   new CsvReader(new StreamReader(file), true))
			{
				int fieldCount = csv.FieldCount;
				string currentNS = "";

				string[] headers = csv.GetFieldHeaders();

				for (int c = 1; c < headers.Length; c++)
				{
					string language = headers[c].ToLower();
					dicts.Add(language, new Dictionary<string, string>());
					languages.Add(language);
				}

				while (csv.ReadNextRecord())
				{
					string key = csv[0];
					if (key.Contains("ns:"))
						currentNS = key.Split(':')[1];

					if (currentNS == project && !key.Contains("ns:"))
						if (!string.IsNullOrWhiteSpace(key))
							for (int i = 1; i < fieldCount; i++)
								dicts[headers[i].ToLower()].Add(key, csv[i]);
				}

			}

			var tp = new TranslationProject(project, masterLanguage, languages.ToArray());
			tp.Dicts = dicts;
			
			return tp;
		}

		public static void ToCSV(TranslationProject project, string targetDir)
		{
			StringBuilder sb = new StringBuilder();
			ToCSV(project, sb, true);

			using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + project.Project + ".csv", false, Encoding.UTF8))
			{
				outfile.Write(sb.ToString());
			}
		}

		public static void ToCSV(TranslationProject project, StringBuilder sb, bool addHeader = true)
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
