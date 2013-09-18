using System.IO;

namespace TranslationTool.IO
{
	public class Xls
	{
		public static void ToXLS(TranslationProject project, string fileName)
		{
			FileInfo newFile = new FileInfo(fileName);
			if (newFile.Exists)
			{
				newFile.Delete();  // ensures we create a new workbook
				newFile = new FileInfo(fileName);
			}
			using (var package = new OfficeOpenXml.ExcelPackage(newFile))
			{
				var worksheet = package.Workbook.Worksheets.Add("Traductions");
				ToXLS(project, worksheet, 1);

				package.Save();
			}
		}

		public static int ToXLS(TranslationProject project, OfficeOpenXml.ExcelWorksheet worksheet, int rowStart = 1)
		{
			int columnCounter = 1;
			int rowCounter = rowStart;

			if (rowStart == 1) //write header
			{
				worksheet.Cells[rowStart, columnCounter++].Value = "";
				worksheet.Cells[rowStart, columnCounter++].Value = project.masterLanguage;
				foreach (var l in project.Languages)
					worksheet.Cells[rowStart, columnCounter++].Value = l;
				rowCounter++;
			}

			foreach (var kvp in project.masterDict)
			{
				columnCounter = 1;
				worksheet.Cells[rowCounter, columnCounter++].Value = kvp.Key;
				worksheet.Cells[rowCounter, columnCounter++].Value = kvp.Value; //write master language
				foreach (var l in project.Languages)
					worksheet.Cells[rowCounter, columnCounter++].Value = project.dicts.ContainsKey(l) ? project.dicts[l].ContainsKey(kvp.Key) ? project.dicts[l][kvp.Key] : "" : "";
				rowCounter++;
			}

			return rowCounter;
		}

	}
}
