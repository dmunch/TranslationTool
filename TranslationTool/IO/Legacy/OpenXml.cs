using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;
using System.IO.Packaging;

namespace TranslationTool.IO
{
	public class SLExcelWriter
	{
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

		private static Cell CreateTextCell(string header, UInt32 index,
			string text)
		{
			var cell = new Cell
			{
				DataType = CellValues.InlineString,
				CellReference = header + index
			};

			var istring = new InlineString();
			var t = new Text { Text = text };
			istring.AppendChild(t);
			cell.AppendChild(istring);
			return cell;
		}

		public static Stream GenerateExcel()
		{
			var stream = new MemoryStream();
			var document = SpreadsheetDocument
				.Create(stream, SpreadsheetDocumentType.Workbook);

			var workbookpart = document.AddWorkbookPart();
			workbookpart.Workbook = new Workbook();

			// Add a new worksheet part to the workbook.
			WorksheetPart newWorksheetPart = document.WorkbookPart.AddNewPart<WorksheetPart>();
			newWorksheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(new SheetData());

			//Sheets sheets = document.WorkbookPart.Workbook.GetFirstChild<Sheets>();
			var sheets = document.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

			string relationshipId = document.WorkbookPart.GetIdOfPart(newWorksheetPart);

			//This bit is required for iPad to be able to read the sheets inside the xlsx file. The file will still work fine in Excel
			string relationshipType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet";
			document.Package.GetPart(document.WorkbookPart.Uri).CreateRelationship(new Uri(newWorksheetPart.Uri.OriginalString.Replace("/xl/", String.Empty).Trim(), UriKind.Relative), TargetMode.Internal, relationshipType);
			document.Package.GetPart(document.WorkbookPart.Uri).DeleteRelationship(relationshipId);
			PackageRelationshipCollection sheetRelationships = document.Package.GetPart(document.WorkbookPart.Uri).GetRelationshipsByType(relationshipType);

			relationshipId = sheetRelationships.Where(f => f.TargetUri.OriginalString == newWorksheetPart.Uri.OriginalString.Replace("/xl/", String.Empty).Trim()).Single().Id;


			// Get a unique ID for the new sheet.
			uint sheetId = 1;
			if (sheets.Elements<Sheet>().Count() > 0)
				sheetId = sheets.Elements<Sheet>().Max(s => s.SheetId.Value) + 1;

			// Append the new worksheet and associate it with the workbook.
			Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = "test" };
			sheets.Append(sheet);

			//worksheets.Add(new Worksheet(newWorksheetPart.Worksheet, sheetId));


			var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
			var sheetData = new SheetData();

			worksheetPart.Worksheet = new Worksheet(sheetData);

			/*
			var sheet = new Sheet()
			{
				Id = document.WorkbookPart
					.GetIdOfPart(worksheetPart),
				SheetId = 1,
				Name = "Sheet 1"
			};
			sheets.AppendChild(sheet);
			*/

			// Add header
			UInt32 rowIdex = 0;
			var row = new Row { RowIndex = ++rowIdex };
			sheetData.AppendChild(row);
			int cellIdex = 0;

			/*
			foreach (var header in data.Headers)
			{
				row.AppendChild(CreateTextCell(ColumnLetter(cellIdex++),
					rowIdex, header ?? string.Empty));
			}
			if (data.Headers.Count > 0)
			{
				// Add the column configuration if available
				if (data.ColumnConfigurations != null)
				{
					var columns = (Columns)data.ColumnConfigurations.Clone();
					worksheetPart.Worksheet
						.InsertAfter(columns, worksheetPart
						.Worksheet.SheetFormatProperties);
				}
			}
			*/

			// Add sheet data
			foreach (var rowData in new [] {new []{"a", "b", "c"}, new []{"d", "e", "f"}})
			{
				cellIdex = 0;
				row = new Row { RowIndex = ++rowIdex };
				sheetData.AppendChild(row);
				foreach (var callData in rowData)
				{
					var cell = CreateTextCell(ColumnLetter(cellIdex++),	rowIdex, callData ?? string.Empty);
					row.AppendChild(cell);
				}
			}

			workbookpart.Workbook.Save();
			document.Close();

			return stream;
		}
	}

	public class Test
	{
		// Given a Worksheet and an address (like "AZ254"), either return a 
		// cell reference, or create the cell reference and return it.
		private Cell InsertCellInWorksheet(Worksheet ws, string addressName)
		{
			SheetData sheetData = ws.GetFirstChild<SheetData>();
			Cell cell = null;

			UInt32 rowNumber = GetRowIndex(addressName);
			Row row = GetRow(sheetData, rowNumber);

			// If the cell you need already exists, return it.
			// If there is not a cell with the specified column name, insert one.  
			Cell refCell = row.Elements<Cell>().
				Where(c => c.CellReference.Value == addressName).FirstOrDefault();
			if (refCell != null)
			{
				cell = refCell;
			}
			else
			{
				cell = CreateCell(row, addressName);
			}
			return cell;
		}

		// Add a cell with the specified address to a row.
		private Cell CreateCell(Row row, String address)
		{
			Cell cellResult;
			Cell refCell = null;

			// Cells must be in sequential order according to CellReference. 
			// Determine where to insert the new cell.
			foreach (Cell cell in row.Elements<Cell>())
			{
				if (string.Compare(cell.CellReference.Value, address, true) > 0)
				{
					refCell = cell;
					break;
				}
			}

			cellResult = new Cell();
			cellResult.CellReference = address;

			row.InsertBefore(cellResult, refCell);
			return cellResult;
		}

		// Return the row at the specified rowIndex located within
		// the sheet data passed in via wsData. If the row does not
		// exist, create it.
		private Row GetRow(SheetData wsData, UInt32 rowIndex)
		{
			var row = wsData.Elements<Row>().
			Where(r => r.RowIndex.Value == rowIndex).FirstOrDefault();
			if (row == null)
			{
				row = new Row();
				row.RowIndex = rowIndex;
				wsData.Append(row);
			}
			return row;
		}

		// Given an Excel address such as E5 or AB128, GetRowIndex
		// parses the address and returns the row index.
		private UInt32 GetRowIndex(string address)
		{
			string rowPart;
			UInt32 l;
			UInt32 result = 0;

			for (int i = 0; i < address.Length; i++)
			{
				if (UInt32.TryParse(address.Substring(i, 1), out l))
				{
					rowPart = address.Substring(i, address.Length - i);
					if (UInt32.TryParse(rowPart, out l))
					{
						result = l;
						break;
					}
				}
			}
			return result;
		}
	}
	
	class OpenXml
	{
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

		private static Cell CreateTextCell(string header, UInt32 index, string text)
		{
			var cell = new Cell
			{
				DataType = CellValues.InlineString,
				CellReference = header + index
			};

			var istring = new InlineString();
			var t = new Text { Text = text };
			istring.AppendChild(t);
			cell.AppendChild(istring);
			return cell;
		}

		public static Stream ToXLSX2(TranslationModule project)
		{
			MemoryStream xlsStream = new MemoryStream();

			var worksheet = new Worksheets.MemoryWorksheet();
			Export.ToIWorksheet(project, worksheet);

			using (SpreadsheetDocument myDoc = SpreadsheetDocument.Create(xlsStream, SpreadsheetDocumentType.Workbook))
			{
				WorkbookPart workbookPart = myDoc.AddWorkbookPart();
				workbookPart.Workbook = new Workbook();	
				
				WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
				SheetData sheetData = new SheetData();
				worksheetPart.Worksheet = new Worksheet(sheetData);
				
				Sheets sheets = myDoc.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
				Sheet sheet = new Sheet()
				{
					Id = myDoc.WorkbookPart.GetIdOfPart(worksheetPart),
					SheetId = 1,
					Name = "mySheet"
				};
				sheets.AppendChild(sheet);

				int cellIdx = 0;
				for (uint row = 0; row < worksheet.Rows; row++)
				{
					var r = new Row();					
					r.RowIndex = row;

					for (int col = 0; col < worksheet.Columns; col++)
					{
						/*
						var c = new Cell();						
						c.DataType = CellValues.String;
						//string val = worksheet[row, col] != null ? worksheet[row, col].ToString() : "";
						string val = "test;";
						c.CellValue = new CellValue(val);
						
						r.InsertAt(c, col);
						*/
						var c = CreateTextCell(ColumnLetter(cellIdx++), row, "test");
						r.AppendChild(c);
					}
					sheetData.AppendChild(r);
					
				}
				
				worksheetPart.Worksheet.Save();
				workbookPart.Workbook.Save();				
				myDoc.Close();
			}

			return xlsStream;
		}



		public static Stream ToXLSX(TranslationModule project)
		{
			MemoryStream xlsStream = new MemoryStream();

			var worksheet = new Worksheets.MemoryWorksheet();
			Export.ToIWorksheet(project, worksheet);
			
			var ss = new Stylesheet();
			ss.CellStyleFormats = new CellStyleFormats();
			ss.CellStyleFormats.Count = 1;
			ss.CellStyles = new CellStyles();
			ss.CellStyles.Count = 1;
			ss.DifferentialFormats = new DifferentialFormats();
			ss.DifferentialFormats.Count = 0;
			ss.TableStyles = new TableStyles();
			ss.TableStyles.Count = 0;

			using (SpreadsheetDocument myDoc = SpreadsheetDocument.Create(xlsStream, SpreadsheetDocumentType.Workbook))
			{
				WorkbookPart workbookPart = myDoc.AddWorkbookPart();
				//workbookPart.Workbook = new Workbook();	
				WorkbookStylesPart stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
				stylesPart.Stylesheet = ss;
				stylesPart.Stylesheet.Save();

				WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
				//SheetData sheetData = new SheetData();
				//worksheetPart.Worksheet = new Worksheet(sheetData);
				/*
				Sheets sheets = myDoc.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
				Sheet sheet = new Sheet()
				{
					Id = myDoc.WorkbookPart.GetIdOfPart(worksheetPart),
					SheetId = 1,
					Name = "mySheet"
				};
				sheets.Append(sheet);
				*/
				OpenXmlWriter writer = OpenXmlWriter.Create(worksheetPart);
				
				
				writer.WriteStartElement(new Worksheet());
				writer.WriteStartElement(new SheetData());
				
				for (int row = 0; row <  worksheet.Rows; row++)				
				{
					var oxa = new List<OpenXmlAttribute>();
					// this is the row index
					oxa.Add(new OpenXmlAttribute("r", null, row.ToString()));
					writer.WriteStartElement(new Row(), oxa);

					for (int col = 0; col < worksheet.Columns; col++)
					{
						oxa = new List<OpenXmlAttribute>();
						// this is the data type ("t"), with CellValues.String ("str")
						oxa.Add(new OpenXmlAttribute("t", null, "str"));
						string val = worksheet[row, col] != null ? worksheet[row, col].ToString() : "";
						
						//var cell = new Cell(new CellValue(val));
						writer.WriteStartElement(new Cell(), oxa);
						
						//Cell c = new Cell();
						CellValue v = new CellValue(val);
						writer.WriteElement(v);
						//c.AppendChild(v);

						
						//writer.WriteElement();
						writer.WriteEndElement();						
					}
					writer.WriteEndElement();
				}
				
				writer.WriteEndElement();
				writer.WriteEndElement();

				writer.Close();

				writer = OpenXmlWriter.Create(myDoc.WorkbookPart);
				writer.WriteStartElement(new Workbook());
				writer.WriteStartElement(new Sheets());

				// you can use object initialisers like this only when the properties
				// are actual properties. SDK classes sometimes have property-like properties
				// but are actually classes. For example, the Cell class has the CellValue
				// "property" but is actually a child class internally.
				// If the properties correspond to actual XML attributes, then you're fine.
				writer.WriteElement(new Sheet()
				{
					Name = "Sheet1",
					SheetId = 1,
					Id = myDoc.WorkbookPart.GetIdOfPart(worksheetPart)
				});

				// this is for Sheets
				writer.WriteEndElement();
				// this is for Workbook
				writer.WriteEndElement();
				writer.Close();

				myDoc.Close();
			}

			return xlsStream;
		}

		static void WriteRandomValuesSAX(string filename, int numRows, int numCols)
		{
			using (SpreadsheetDocument myDoc = SpreadsheetDocument.Open(filename, true))
			{
				WorkbookPart workbookPart = myDoc.WorkbookPart;
				WorksheetPart worksheetPart = workbookPart.WorksheetParts.Last();

				OpenXmlWriter writer = OpenXmlWriter.Create(worksheetPart);

				Row r = new Row();
				Cell c = new Cell();
				CellValue v = new CellValue("Test");
				c.AppendChild(v);

				writer.WriteStartElement(new Worksheet());
				writer.WriteStartElement(new SheetData());
				for (int row = 0; row < numRows; row++)
				{
					writer.WriteStartElement(r);
					for (int col = 0; col < numCols; col++)
					{
						writer.WriteElement(c);
					}
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
				writer.WriteEndElement();

				writer.Close();
			}
		}
	}
}
