using System;
using System.Collections.Generic;
using Google.Apis.Drive.v2.Data;
using Google.GData.Client;
using Google.GData.Spreadsheets;

#if false 

namespace TranslationTool.IO.Google.Tests
{
	class SpreadsheetsTests
	{
		public static void Test()
		{
			var driveService = Drive.GetService();

			//create a file
			//http://markembling.info/2012/12/google-spreadsheet-dotnet-oauth2-service-account

			var file = new File();

			file.Title = "Test spreadsheet";
			file.Description = string.Format("Created via {0} at {1}", Drive.ApplicationName, DateTime.Now.ToString());
			file.MimeType = "application/vnd.google-apps.spreadsheet";


			var request = driveService.Files.Insert(file);
			var result = request.Execute();
			var spreadsheetLink = "https://spreadsheets.google.com/feeds/spreadsheets/" + result.Id;

			// SPREADSHEETS - edit the newly created spreadsheet
			var spreadsheetsService = Spreadsheets.GetService();

			// query for the one we just made
			Console.WriteLine("List the one we just made up above here:");
			var query = new SpreadsheetQuery(spreadsheetLink);
			var feed = spreadsheetsService.Query(query);
			var sheet = (SpreadsheetEntry)feed.Entries[0];
			Console.WriteLine(sheet.Title.Text);

			// Do some stuff with it
			var worksheetsFeed = sheet.Worksheets;
			var worksheet = (WorksheetEntry)worksheetsFeed.Entries[0];

			worksheet.Title.Text = "Created by " + Drive.ApplicationName;
			//            worksheet.Cols = 10;
			//            worksheet.Rows = 8;
			worksheet.Update();

			// Data via cell API - works but one update request per cell
			var cellQuery = new CellQuery(worksheet.CellFeedLink);
			cellQuery.ReturnEmpty = ReturnEmptyCells.yes;
			var cellFeed = spreadsheetsService.Query(cellQuery);

			foreach (CellEntry cell in cellFeed.Entries)
			{
				Console.WriteLine("cell");
				Console.WriteLine(cell.Title.Text);
				switch (cell.Title.Text)
				{
					case "A1":
						cell.InputValue = "Joe";
						cell.Update();
						break;
					case "A2":
						cell.InputValue = "Bloggs";
						cell.Update();
						break;
					default:
						break;
				}
			}

			// Via cell API, bulk update - more efficient
			DoBulkUpdate(spreadsheetsService, worksheet);

			Console.WriteLine("Done");
			Console.ReadLine();
		}

		private class CellIdentifier
		{
			public uint Row { get; private set; }
			public uint Col { get; private set; }

			public CellIdentifier(uint row, uint col)
			{
				Row = row;
				Col = col;
			}

			public string Id { get { return string.Format("R{0}C{1}", Row, Col); } }
		}

		private static void DoBulkUpdate(SpreadsheetsService service, WorksheetEntry worksheet)
		{

			Console.WriteLine("Attempting batch update...");

			var cellQuery = new CellQuery(worksheet.CellFeedLink); //new CellQuery(result.Id, "od6", "private", "full");
			var cellFeed = service.Query(cellQuery);

			// Work out those we wish to update
			var cellIdentifiers = new List<CellIdentifier>();
			for (uint row = 3; row <= 5; row++)
			{
				for (uint col = 1; col <= 3; col++)
				{
					cellIdentifiers.Add(new CellIdentifier(row, col));
				}
			}

			// Query Google's API for these cells...
			CellFeed cellsToUpdate = BatchQuery(service, cellFeed, cellIdentifiers);

			// Update query...
			CellFeed batchRequest = new CellFeed(cellQuery.Uri, service);
			foreach (CellEntry cell in cellsToUpdate.Entries)
			{
				CellEntry batchEntry = cell;
				batchEntry.InputValue = string.Format("hello {0}", cell.BatchData.Id);
				batchEntry.BatchData = new GDataBatchEntryData(cell.BatchData.Id, GDataBatchOperationType.update);
				batchRequest.Entries.Add(batchEntry);
			}

			var batchResponse = (CellFeed)service.Batch(batchRequest, new Uri(cellFeed.Batch));

			// Check all is well
			foreach (CellEntry entry in batchResponse.Entries)
			{
				var currentCellId = entry.BatchData.Id;
				if (entry.BatchData.Status.Code == 200)
				{
					Console.WriteLine(string.Format("Cell {0} succeeded", currentCellId));
				}
				else
				{
					Console.WriteLine(string.Format("Cell {0} failed: (status {1}) {2}", currentCellId, entry.BatchData.Status.Code, entry.BatchData.Status.Reason));
				}
			}
		}

		private static CellFeed BatchQuery(SpreadsheetsService service, CellFeed cellFeed, IEnumerable<CellIdentifier> cellIdentifiers)
		{
			CellFeed batchRequest = new CellFeed(new Uri(cellFeed.Self), service);
			foreach (var cellId in cellIdentifiers)
			{
				CellEntry batchEntry = new CellEntry(cellId.Row, cellId.Col, cellId.Id);
				batchEntry.Id = new AtomId(string.Format("{0}/{1}", cellFeed.Self, cellId.Id));
				batchEntry.BatchData = new GDataBatchEntryData(cellId.Id, GDataBatchOperationType.query);
				batchRequest.Entries.Add(batchEntry);
			}

			CellFeed queryBatchResponse = (CellFeed)service.Batch(batchRequest, new Uri(cellFeed.Batch));

			return queryBatchResponse;
		}
	}
}
#endif