using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Spreadsheets;
using Google.GData.Client;
#if false

namespace TranslationTool.IO
{
	public class GDataSpreadSheet
	{
		public static TranslationModule FromGDoc(string userName, string password, string project)
		{
			var g = new GDataSpreadSheet(userName, password);

			var ws = g.QueryWorksheet(project);
			return g.FromWorksheet(project, ws);
		}

		public static TranslationModule FromGDoc(string project)
		{
			var g = new GDataSpreadSheet();

			var ws = g.QueryWorksheet(project);
			return g.FromWorksheet(project, ws);
		}

		SpreadsheetsService Service;

		public void Test()
		{
			WorksheetEntry ws = QueryWorksheet("_WFIGGO_FIACCUEIL_FICOMMON");
			CellQuery(ws);
		}

		public GDataSpreadSheet()
		{
			this.Service = Google.Spreadsheets.GetService();
		}

		public GDataSpreadSheet(string userName, string password)
		{
			this.Service = new SpreadsheetsService("MySpreadsheetIntegration-v1");
            //this.Service.SetAuthenticationToken("-DQAAAOQAAADXyTBmxLMugfFLvQ1X019nvotwxaxL2wSYSp0rTmPIbbiCt6SpfYMd0AJrgL58S2m92avxMj5lQVASfD30RYRtTDXp2XrMhVFa5GYKbh_hJwTvFKmc3cqJM1FS61bIh9w-b0lQ1TjlzWsffiHr8R4wv3gV24_lScn0tkxrI-84mMU27gyaoWtPlof5ynw9LujgKrekcLlXfYNJZDlIwa9DWGtif8JG1S0Sx92S8OjO1UaYASBh-4WOJ8q9Imx-Y12BxJ4faNESoji_jpXlYonr6kj3IGTWzSGITYZBpX04ieJqojMBkUIjav8n-Js_aI8");
			this.Service.setUserCredentials(userName, password);
            //string token = this.Service.Credentials.ClientToken;
		}
		
		public WorksheetEntry QueryWorksheet(string title)
		{
		
			SpreadsheetQuery query = new SpreadsheetQuery();
			query.Title = title;

			SpreadsheetFeed feed = this.Service.Query(query);

			if (feed.Entries.Count == 0)
			{
				// TODO: There were no spreadsheets, act accordingly.
			}

			// TODO: Choose a spreadsheet more intelligently based on your
			// app's needs.
			SpreadsheetEntry spreadsheet = (SpreadsheetEntry)feed.Entries[0];
			WorksheetFeed wsFeed = spreadsheet.Worksheets;
			WorksheetEntry worksheet = (WorksheetEntry)wsFeed.Entries[0];

			return worksheet;
		}
		
		protected TranslationModule FromWorksheet(string project, WorksheetEntry worksheet)
		{
			// Define the URL to request the list feed of the worksheet.
			AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

			// Fetch the list feed of the worksheet.
			ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());		
			ListFeed listFeed = this.Service.Query(listQuery);

			Console.WriteLine("Results {0}", listFeed.Entries.Count);

			List<string> languages = new List<string>();
			var dicts = new Dictionary<string, Dictionary<string, string>>();
			var comments = new Dictionary<string, string>();

			//language information is in first row
			var firstRow = (ListEntry)listFeed.Entries[0];

			for (int column = 1; column < firstRow.Elements.Count; column++)
			{
				var lang = firstRow.Elements[column].LocalName;
				languages.Add(lang);
				dicts.Add(lang, new Dictionary<string, string>());
			}

			var tp = new TranslationModule(project, "en", languages.ToArray());
			for (int rowIdx = 1; rowIdx < listFeed.Entries.Count; rowIdx++)
			{
				ListEntry row = (ListEntry)listFeed.Entries[rowIdx];
				string comment = null;

				string key = row.Elements[0].Value; // first column is the key
				// Update the row's data.
				for (int column = 1; column < row.Elements.Count; column++)
				{
					var element = row.Elements[column];

					//in list based feeds localname always correponds to first row
					if (element.LocalName.ToLower() == "comment")
					{
						comment = element.Value;
					}
					else
					{ 						
						tp.Add(new Segment(element.LocalName, key, element.Value));
					}
				}

				foreach(var seg in tp.ByKey[key])
				{
					seg.Comment = comment;
				}
			}

			return tp;
		}

		public void ListQuery(WorksheetEntry worksheet)
		{
			// Define the URL to request the list feed of the worksheet.
			AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

			// Fetch the list feed of the worksheet.
			ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
			//listQuery.MinPublication = new DateTime(2013, 9, 18);
			//listQuery.ModifiedSince = new DateTime(2013, 9, 18);
			listQuery.StartDate = new DateTime(2013, 9, 18);

			ListFeed listFeed = this.Service.Query(listQuery);

			Console.WriteLine("Results {0}", listFeed.Entries.Count);

			for (int rowIdx = 0; rowIdx < 10; rowIdx++)
			{
				// TODO: Choose a row more intelligently based on your app's needs.
				ListEntry row = (ListEntry)listFeed.Entries[rowIdx];


				// Update the row's data.
				foreach (ListEntry.Custom element in row.Elements)
				{

					Console.WriteLine(element.LocalName + " : " + element.Value);
					Console.ReadLine();
				}
			}
		}

		public void CellQuery(WorksheetEntry worksheet)
		{
			// Fetch the cell feed of the worksheet.
			CellQuery cellQuery = new CellQuery(worksheet.CellFeedLink);
			//cellQuery.ModifiedSince = new DateTime(2013, 9, 1);
			//cellQuery.StartDate = new DateTime(2013, 9, 18);
			//cellQuery.MinPublication = new DateTime(2013, 9, 18);

			CellFeed cellFeed = this.Service.Query(cellQuery);

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
	}
}
#endif