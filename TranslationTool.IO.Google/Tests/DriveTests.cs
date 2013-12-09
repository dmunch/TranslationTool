using System;
using Google.Apis.Drive.v2.Data;

namespace TranslationTool.IO.Google.Tests
{
	class DriveTests
	{
		public static void Revision()
		{
			var driveService = Drive.GetService();
			var t = driveService.Revisions.Get("test", "test").ExecuteAsync().Result;

			var list = driveService.Files.List();

			var file = driveService.Files.Get("test");
		}


		public static void Upload(TranslationModule tp)
		{
			/*
			var xlsStream = IO.XlsX.ToXLSX(tp);
			var filename = @"D:\Users\login\Documents\i18n\TTTT.xlsx";
			
			IO.XlsX.ToXLSX(tp, filename);
			

			xlsStream = IO.XlsX.ToXLS(tp);
			IO.XlsX.ToXLS(tp,@"D:\Users\login\Documents\i18n\TTTT.xls");
			xlsStream.Seek(0, System.IO.SeekOrigin.Begin);

			StringBuilder csv = new StringBuilder();
			IO.CSV.ToCSV(tp, csv, true, ',');
			xlsStream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(csv.ToString()));

			xlsStream = OpenXml.ToXLSX2(tp);
			 * */

			var xlsStream = SLExcelWriter.GenerateExcel();
			xlsStream.Seek(0, System.IO.SeekOrigin.Begin);

			var driveService = Drive.GetService();

			var file = new File();

			file.Title = tp.Name;
			file.Description = string.Format("Created via {0} at {1}", Drive.ApplicationName, DateTime.Now.ToString());
			file.MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
			//file.MimeType = "application/vnd.ms-excel";
			//file.MimeType = "text/csv";
			//file.MimeType = "";
			//file.OriginalFilename = tp.Project + ".xls";
			//var request = driveService.Files.Insert(file, new System.IO.MemoryStream(Encoding.UTF8.GetBytes(csv.ToString())), "text/csv");
			var request = driveService.Files.Insert(file, xlsStream, file.MimeType);

			//var request = driveService.Files.Insert(file, IO.Xls.ToXLS(tp), "text/csv");
			request.Convert = true;

			var r = request.UploadAsync().Result;

			//var result = request.Execute();

			/*
			file = new File();
			file.Title = "Test spreadsheet";
			file.Description = string.Format("Created via {0} at {1}", ApplicationName, DateTime.Now.ToString());
			file.MimeType = "application/vnd.google-apps.spreadsheet";


			var request2 = driveService.Files.Insert(file);
			var result = request2.Execute();			
			*/

			using (var fileStream = System.IO.File.Create(@"D:\Users\login\Documents\i18n\TTTT.xlsx"))
			{
				xlsStream.Seek(0, System.IO.SeekOrigin.Begin);
				xlsStream.CopyTo(fileStream);
			}
		}		
	}
}
