using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslationTool.IO
{
	public interface IWorksheet
	{
		int Columns { get; }
		int Rows { get; }

		object this[int row, int column]
		{
			get;
			set;
		}
	}
	public static class Export
	{
		public static TranslationModule FromIWorksheet(string project, string masterLanguage, IWorksheet worksheet)
		{
			// open the file "data.csv" which is a CSV file with headers
			TranslationModule tp = null;
			bool createMissingKeys = false;

			Dictionary<int, string> languages = new Dictionary<int, string>();
			
			string currentNS = project;
			int commentColumn = -1;

			for (int c = 1; c < worksheet.Columns; c++)
			{
				string language = ((string)worksheet[0, c]).ToLower();
				if (language == "Comment")
				{
					commentColumn = c;
					continue;
				}

				languages.Add(c, language);
			}
			tp = new TranslationModule(project, masterLanguage, languages.Values.ToArray());

			for (int r = 1; r < worksheet.Rows; r++)
			{
				string key = (string)worksheet[r, 0];
				if (string.IsNullOrWhiteSpace(key)) continue;

				if (key.Contains("ns:"))
					currentNS = key.Split(':')[1];

				if (currentNS == project && !key.Contains("ns:"))
				{
					if (string.IsNullOrWhiteSpace(key) && createMissingKeys)
					{
						string keyInspiration = commentColumn != 1 ? (string)worksheet[r, 1] : (string)worksheet[r, 2];
						key = tp.KeyProposal(keyInspiration);
					}
					if (!string.IsNullOrWhiteSpace(key))
					{
						for (int c = 1; c < languages.Count + 1; c++)
						{
							tp.Add(new Segment(languages[c], key, (string)worksheet[r, c]));
							/*
							if (c != commentColumn)
							{
								tp.Dicts[languages[c].ToLower()].Add(key, (string) worksheet[r, c]);
							}
							else
							{
								tp.Comments.Add(key, (string) worksheet[r, c]);
							}*/
					}
					}
				}								
			}

			return tp;
		}

		public static int ToIWorksheet(TranslationModule project, IWorksheet worksheet, int rowStart = 0)
			{
				int columnCounter = 0;
				int rowCounter = rowStart;

				if (rowStart == 0) //write header
				{
					worksheet[rowStart, columnCounter++] = "";

					foreach (var l in project.Languages)
						worksheet[rowStart, columnCounter++] = l;
					rowCounter++;
				}

				worksheet[rowCounter++, 0] = "ns:" + project.Name;

				var byLanguageAndKey = project.ByLanguageAndKey;
				var byKey = project.ByKey;
				foreach (var key in project.Keys)
				{
					columnCounter = 0;
					worksheet[rowCounter, columnCounter++] = key;
					
					//need to write by languages to make sure we have empty cells!!
					foreach (var l in project.Languages)
					{
						var seg = byKey[key].FirstOrDefault(s => s.Language == l);
						worksheet[rowCounter, columnCounter++] = seg != null ? seg.Text : "";
					}
					rowCounter++;
				}

				return rowCounter;
			}
	}
}
