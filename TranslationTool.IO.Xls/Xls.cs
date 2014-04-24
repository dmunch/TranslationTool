using System.IO;
using ExcelLibrary.SpreadSheet;

namespace TranslationTool.IO
{
	public class Xls
	{
		protected static Workbook _ToXLS(ITranslationModule project)
		{
			Worksheet worksheet = new Worksheet("Traductions");
			Export.ToIWorksheet(project, new XlsWorksheet(worksheet));

			Workbook workbook = new Workbook();
			workbook.Worksheets.Add(worksheet);

			return workbook;
		}

		public static void ToXLS(ITranslationModule project, string fileName)
		{
			FileInfo newFile = new FileInfo(fileName);
			if (newFile.Exists)
			{
				newFile.Delete();  // ensures we create a new workbook
			}

			_ToXLS(project).Save(fileName);
		}

		public static Stream ToXLS(ITranslationModule project)
		{
			MemoryStream stream = new MemoryStream();
			_ToXLS(project).SaveToStream(stream);

			return stream;
		}

		public static string ToXlsX(string fileName)
		{
			const string sOfficePath = @"C:\Program Files (x86)\LibreOffice 4.0\program\soffice.exe";

			return ToXlsX(fileName, sOfficePath);
		}

		public static string ToXlsX(string fileName, string sOfficePath)
		{
			string arg = "--headless --convert-to xlsx " + fileName;

			var p2 = new System.Diagnostics.Process();
			p2.StartInfo = new System.Diagnostics.ProcessStartInfo();
			p2.StartInfo.Arguments = arg;
			p2.StartInfo.FileName = sOfficePath;
			p2.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(fileName);
			p2.Start();
			p2.WaitForExit();
			return p2.StartInfo.WorkingDirectory + @"\" + System.IO.Path.GetFileNameWithoutExtension(fileName) + ".xlsx";
		}
	}

	public class XlsWorksheet : IWorksheet
	{
		Worksheet Ws;
		public XlsWorksheet(Worksheet ws)
		{
			this.Ws = ws;
		}

		public int Columns { get { return Ws.Cells.LastColIndex; } }
		public int Rows { get { return Ws.Cells.LastRowIndex; } }

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
