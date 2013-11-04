using System.IO;
using ExcelLibrary.SpreadSheet;

namespace TranslationTool.IO
{
	class Xls
	{
		protected static Workbook _ToXLS(TranslationProject project)
		{
			Worksheet worksheet = new Worksheet("Traductions");
			Export.ToIWorksheet(project, new XlsWorksheet(worksheet), 1);

			Workbook workbook = new Workbook();
			workbook.Worksheets.Add(worksheet);

			return workbook;
		}

		public static void ToXLS(TranslationProject project, string fileName)
		{
			FileInfo newFile = new FileInfo(fileName);
			if (newFile.Exists)
			{
				newFile.Delete();  // ensures we create a new workbook
			}

			_ToXLS(project).Save(fileName);
		}

		public static Stream ToXLS(TranslationProject project)
		{
			MemoryStream stream = new MemoryStream();
			_ToXLS(project).SaveToStream(stream);

			return stream;
		}
	}


	public class XlsWorksheet : IWorksheet
	{
		Worksheet Ws;
		public XlsWorksheet(Worksheet ws)
		{
			this.Ws = ws;
		}

		public object this[int row, int column]
		{
			get
			{
				return this.Ws.Cells[row, column].Value;
			}
			set
			{
				this.Ws.Cells[row, column] = new Cell(value);
			}
		}
	}
}
