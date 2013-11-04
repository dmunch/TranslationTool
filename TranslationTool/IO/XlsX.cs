using System.IO;

namespace TranslationTool.IO
{		
	public class XlsX
	{
		public static void ToXLSX(TranslationProject project, string fileName)
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
				Export.ToIWorksheet(project, new OpenXmlWorksheet(worksheet), 1);

				package.Save();				
			}
		}

		public static Stream ToXLSX(TranslationProject project)
		{			
			MemoryStream xlsStream = new MemoryStream();
			using (var package = new OfficeOpenXml.ExcelPackage(xlsStream))
			{
				var worksheet = package.Workbook.Worksheets.Add("Traductions");
				Export.ToIWorksheet(project, new OpenXmlWorksheet(worksheet), 1);

				package.Save();
			}

			return xlsStream;
		}				
	}

	public class OpenXmlWorksheet : IWorksheet
	{
		OfficeOpenXml.ExcelWorksheet Ws;
		public OpenXmlWorksheet(OfficeOpenXml.ExcelWorksheet worksheet)
		{
			this.Ws = worksheet;
		}

		public object this[int row, int column]
		{
			get
			{
				return this.Ws.Cells[row, column].Value;
			}
			set
			{
				this.Ws.Cells[row, column].Value = value;
			}
		}
	}
}
