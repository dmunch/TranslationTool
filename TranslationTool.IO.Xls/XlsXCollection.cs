﻿using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace TranslationTool.IO.Collection
{
	public class XlsX
	{
		public static TranslationProject FromMultiSpreadsheet(string masterLanguage, Stream stream)
		{
			var tpc = new TranslationProject();

			using (var package = new OfficeOpenXml.ExcelPackage())
			{
				package.Load(stream);
				foreach(var worksheet in package.Workbook.Worksheets)
				{
					var project = Export.FromIWorksheet(worksheet.Name, masterLanguage, new XlsXWorksheet(worksheet));
					tpc.Projects.Add(worksheet.Name, project);
				}				
			}

			return tpc;
		}

		public void ToOneXLSX(TranslationProject tpc, string targetDir)
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
				int columnCount = 0;
				worksheet.Cells[0, columnCount++].Value = "";
				worksheet.Cells[0, columnCount++].Value = tpc.Projects.First().Value.MasterLanguage;

				foreach (var l in tpc.Projects.First().Value.Languages)
					worksheet.Cells[0, columnCount++].Value = l;

				int rowCount = 1;
				foreach (var kvp in tpc.Projects)
				{
					worksheet.Cells[rowCount, 0].Value = "ns:" + kvp.Key;
					rowCount++;

					rowCount = IO.Export.ToIWorksheet(kvp.Value, new XlsXWorksheet(worksheet), rowCount);
				}

				package.Save();
			}

		}
	}
}
