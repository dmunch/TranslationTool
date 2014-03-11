using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Drive.v2.Data;

namespace TranslationTool.Lib
{
	public class Downloader
	{
		public bool AddKeyName { get; set; }
		public bool AddModuleName { get; set; }
		public bool UseMultiSpreadsheet { get; set; }
		public Downloader()
		{
			AddKeyName = false;
			AddModuleName = false;
			UseMultiSpreadsheet = false;
		}
	
		public void Download(string googleDriveFolderName, string localFolderName)
		{
			//var drive = new IO.Google.Drive2(IO.Google.Drive.GetServiceAccountService());
			var drive = new IO.Google.Drive2(IO.Google.Drive.GetService());
			
			var folder = drive.FindFolder(googleDriveFolderName);
			var spreadsheets = drive.FindSpreadsheetFiles(folder);

			var files = new Dictionary<File, System.IO.Stream>();
			foreach (File spreadsheet in spreadsheets)
			{
				Console.WriteLine("Downloading {0}...", spreadsheet.Title);
				files.Add(spreadsheet, drive.DownloadFile(spreadsheet, true));

				//sleep a second
				System.Threading.Thread.Sleep(5000);
			}

			IEnumerable<TranslationModule> modules;

			if (UseMultiSpreadsheet)
			{
				modules = files.Values.SelectMany(file => FromMultiSpreadsheet(file));
			}
			else
			{
				modules = files.Select(fileKvp => FromFirstSpreadsheet(fileKvp.Value, fileKvp.Key.Title));
			}

			//check if we've got duplicate module names which would result in conflicing file names
			bool hasModuleNameDuplicates = modules.GroupBy(m => m.Name).Where(g => g.Skip(1).Any()).Any();
			if (hasModuleNameDuplicates)
			{
				throw new Exception("Module names aren't unique, did'nt write any files.");
			}

			//Write modules to disk	
			foreach(var module in modules)
			{
				IO.ResX.ToResX(module, localFolderName);
			}
		}

		protected static IEnumerable<TranslationModule> FromMultiSpreadsheet(System.IO.Stream xlsx)
		{
			var projects = IO.Collection.XlsX.FromMultiSpreadsheet("en", xlsx);

			foreach (var project in projects.Projects.Where(p => !p.Key.StartsWith("_")))
			{
				yield return project.Value;
			}
		}

		protected static TranslationModule FromFirstSpreadsheet(System.IO.Stream xlsx, string moduleName)
		{
			return IO.XlsX.FromXLSX(moduleName, "en", xlsx);			
		}
	}
}
