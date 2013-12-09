using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Spreadsheets;
using Google.GData.Client;

namespace TranslationTool.IO.Worksheets
{
	public class GoogleWorksheet : IWorksheet
	{
		WorksheetEntry Worksheet;
		SpreadsheetsService Service;

		IEnumerable<CellEntry> Entries;

		public GoogleWorksheet(WorksheetEntry worksheet)
		{
			Service = (SpreadsheetsService)worksheet.Service;

			// Fetch the cell feed of the worksheet.
			CellQuery cellQuery = new CellQuery(worksheet.CellFeedLink);			
			CellFeed cellFeed = Service.Query(cellQuery);

			this.Columns = cellFeed.RowCount.IntegerValue;
			this.Rows = cellFeed.ColCount.IntegerValue;

			// Iterate through each cell, printing its value.
			foreach (CellEntry cell in cellFeed.Entries)
			{
				Console.WriteLine(cell.Edited.DateValue);
				
				// Print the cell's address in A1 notation
				Console.WriteLine(cell.Title.Text);
				// Print the cell's address in R1C1 notation
				Console.WriteLine(cell.Id.Uri.Content.Substring(cell.Id.Uri.Content.LastIndexOf("/") + 1));
				// Print the cell's formula or text value
				Console.WriteLine(cell.InputValue);
				// Print the cell's calculated value if the cell's value is numeric
				// Prints empty string if cell's value is not numeric
				Console.WriteLine(cell.NumericValue);
				// Print the cell's displayed value (useful if the cell has a formula)
				Console.WriteLine(cell.Value);

				Console.ReadLine();
			}
		}

		public int Columns
		{
			get;
			set;
		}

		public int Rows
		{
			get;
			set;
		}

		public object this[int row, int column]
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}
	}
}
