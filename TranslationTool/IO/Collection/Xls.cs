using System.IO;
using System.Linq;

namespace TranslationTool.IO.Collection
{
	class Xls
	{
		public void ToXLS(TranslationProjectCollection tpc, string targetDir)
		{
			string fileName = @"\";
			foreach (var kvp in tpc.Projects)
				fileName += "_" + kvp.Key;
			fileName += ".xlsx";

			FileInfo newFile = new FileInfo(targetDir + fileName);
			if (newFile.Exists)
			{
				newFile.Delete();  // ensures we create a new workbook
				newFile = new FileInfo(targetDir + fileName);
			}

			using (var package = new OfficeOpenXml.ExcelPackage(newFile))
			{
				var worksheet = package.Workbook.Worksheets.Add("Traductions");

				//write header
				int columnCount = 1;
				worksheet.Cells[1, columnCount++].Value = "";
				worksheet.Cells[1, columnCount++].Value = tpc.Projects.First().Value.MasterLanguage;

				foreach (var l in tpc.Projects.First().Value.Languages)
					worksheet.Cells[1, columnCount++].Value = l;

				int rowCount = 2;
				foreach (var kvp in tpc.Projects)
				{
					worksheet.Cells[rowCount, 1].Value = "ns:" + kvp.Key;
					rowCount++;

					rowCount = IO.Xls.ToXLS(kvp.Value, worksheet, rowCount);
				}

				package.Save();
			}

		}
	}
}
