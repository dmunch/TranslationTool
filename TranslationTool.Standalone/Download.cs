using Google.Apis.Drive.v2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TranslationTool;

namespace TranslationTool.Standalone
{
	class Downloader
	{
		protected IO.Google.Drive2 drive;

		public Downloader(IO.Google.Drive2 drive)
		{
			this.drive = drive;
		}


		public System.IO.Stream DownloadXlsx(BaseDownloadOptions options)
		{
			File spreadsheet = null;
			if (options.GDriveFolder != null)
			{
				var folder = drive.FindFolder(options.GDriveFolder);
				var spreadsheets = drive.FindSpreadsheetFiles(folder);

				if (options.Verbose)
				{
					Console.WriteLine("Available modules in '{0}':", options.GDriveFolder);
					foreach (var file in spreadsheets)
					{
						Console.WriteLine(file.Title);
					}
				}


				spreadsheet = spreadsheets.SingleOrDefault(ss => ss.Title == options.FileName);
			}
			else
			{
				spreadsheet = drive.FindSpreadsheetFile(options.FileName);
			}

			if (spreadsheet == null)
			{
				Console.WriteLine("Module {0} not found. Returning.", options.FileName);
			}

			return drive.DownloadFile(spreadsheet, true);	
		}

		public void Download(DownloadOptions options)
		{
			var xlsx = DownloadXlsx(options);

			if (!options.MultiSpreadsheet)
			{
				var project = IO.XlsX.FromXLSX(options.FileName, "en", xlsx);
				project.Name = options.ModuleName;

				ToOutputFormat(options, project);
			}
			else if (options.MultiSpreadsheet && options.ModuleName != null)
			{
				//A module was specified, only export this one
				var projects = IO.Collection.XlsX.FromMultiSpreadsheet("en", xlsx);
				var project = projects.Projects.Where(p => p.Key == options.ModuleName).Single().Value;
				
				ToOutputFormat(options, project.FilterByTags(options.Tags));
			}
			else if (options.MultiSpreadsheet && options.ModuleName == null)
			{
				//If no moduleName is specified, export all sheets

				var projects = IO.Collection.XlsX.FromMultiSpreadsheet("en", xlsx);				
				foreach (var project in projects.Projects.Where(p => !p.Key.StartsWith("_")))
				{
					ToOutputFormat(options, project.Value.FilterByTags(options.Tags));
				}
			}
		}
		
		protected void ToOutputFormat(DownloadOptions options, ITranslationModule module)
		{
			if (options.Angular)
			{
				IO.Angular.ToAngular(module, options.ResXDir);
			}
			else
			{
				IO.ResX.ToResX(module, options.ResXDir);
			}
		}
	}
}
