using System.IO;

namespace TranslationTool.IO
{		
	public class XlsX
	{

		public static TranslationProject FromXLSX(string project, string masterLanguage, Stream stream)
		{			
			using (var package = new OfficeOpenXml.ExcelPackage())
			{
				package.Load(stream);
				var worksheet = package.Workbook.Worksheets["Traductions"];
				return Export.FromIWorksheet(project, masterLanguage, new OpenXmlWorksheet(worksheet));
			}
		}

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

		public int Columns { get { return Ws.Dimension.End.Column - Ws.Dimension.Start.Column; } }
		public int Rows { get { return Ws.Dimension.End.Row - Ws.Dimension.Start.Row; } }

		public object this[int row, int column]
		{
			get
			{
				return this.Ws.Cells[Address(row, column)].Value;
			}
			set
			{
				this.Ws.Cells[Address(row, column)].Value = value;
			}
		}

		private static string Address(int row, int column)
		{
			var col = ColumnLetter(column);
			return string.Format("{0}{1}", col, row + 1);
		}

		private static string ColumnLetter(int intCol)
		{
			var intFirstLetter = ((intCol) / 676) + 64;
			var intSecondLetter = ((intCol % 676) / 26) + 64;
			var intThirdLetter = (intCol % 26) + 65;

			var firstLetter = (intFirstLetter > 64)
				? (char)intFirstLetter : ' ';
			var secondLetter = (intSecondLetter > 64)
				? (char)intSecondLetter : ' ';
			var thirdLetter = (char)intThirdLetter;

			return string.Concat(firstLetter, secondLetter,
				thirdLetter).Trim();
		}
	}
}
