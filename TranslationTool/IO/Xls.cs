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

				foreach (var l in project.Languages)
					worksheet.Cells[rowStart, columnCounter++].Value = l;
				rowCounter++;
			}

			foreach (var key in project.Keys)
			{
				columnCounter = 1;
				worksheet.Cells[rowCounter, columnCounter++].Value = key;
				
				foreach (var l in project.Languages)
					worksheet.Cells[rowCounter, columnCounter++].Value = project.Dicts.ContainsKey(l) ? project.Dicts[l].ContainsKey(key) ? project.Dicts[l][key] : "" : "";
				rowCounter++;
			}

			return rowCounter;
		}

	}
}
