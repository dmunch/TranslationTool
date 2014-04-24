using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Drive.v2.Data;
using TranslationTool;

namespace TranslationTool.Lib
{
	public class Downloader
	{
		public bool PrefixKeyName { get; set; }
		public bool PrefixModuleName { get; set; }
		public bool UseMultiSpreadsheet { get; set; }

		/// <summary>
		/// (url, resx, name, value) => return "toto";
		/// </summary>
		public Func<string, string, string, string, string> Formatter { get; set; }

		public Downloader()
		{
			PrefixKeyName = false;
			PrefixModuleName = false;
			UseMultiSpreadsheet = false;
		}
	
		public void Download(string googleDriveFolderName, string localFolderName)
		{
			//var drive = new IO.Google.Drive2(IO.Google.Drive.GetServiceAccountService());
			var drive = new TranslationTool.IO.Google.Drive2(TranslationTool.IO.Google.Drive.GetServiceAccountCredential());
			
			var folder = drive.FindFolder(googleDriveFolderName);
			var spreadsheets = drive.FindSpreadsheetFiles(folder);

			var files = new Dictionary<File, System.IO.Stream>();
			foreach (File spreadsheet in spreadsheets)
			{
				Console.WriteLine("Downloading {0}...", spreadsheet.Title);
				files.Add(spreadsheet, drive.DownloadFile(spreadsheet, true));
			}

			Dictionary<File, TranslationModule> modules = new Dictionary<File, TranslationModule>();
			if (UseMultiSpreadsheet)
			{
				foreach(var file in files)
				{
					foreach (var spreadsheet in FromMultiSpreadsheet(file.Value))
						modules.Add(file.Key, new TranslationModule(spreadsheet));
				}
				//modules = files.Values.SelectMany(file => FromMultiSpreadsheet(file));
			}
			else
			{
				foreach (var file in files)
				{					
					modules.Add(file.Key, FromFirstSpreadsheet(file.Value, file.Key.Title));
				}				
			}

			//check if we've got duplicate module names which would result in conflicing file names
			bool hasModuleNameDuplicates = modules.Values.GroupBy(m => m.Name).Where(g => g.Skip(1).Any()).Any();
			if (hasModuleNameDuplicates)
			{
				throw new Exception("Module names aren't unique, did'nt write any files.");
			}

			//Write modules to disk	
			foreach(var module in modules)
			{
				if(this.PrefixKeyName)
				{
					module.Value.AddKeyNamePrefix();
				}

				if (this.PrefixModuleName)
				{
					module.Value.AddPrefix(module.Value.Name);
				}

				if (this.Formatter != null)
				{
					module.Value.Format((resx, name, value) => this.Formatter(module.Key.AlternateLink, resx, name, value));					
				}

				TranslationTool.IO.ResX.ToResX(module.Value, localFolderName);
			}
		}

		protected static IEnumerable<ITranslationModule> FromMultiSpreadsheet(System.IO.Stream xlsx)
		{
			var projects = TranslationTool.IO.Collection.XlsX.FromMultiSpreadsheet("en", xlsx);
			return projects.Projects.Where(p => !p.Key.StartsWith("_")).Select(kvp => kvp.Value);
		}

		protected static TranslationModule FromFirstSpreadsheet(System.IO.Stream xlsx, string moduleName)
		{
			return TranslationTool.IO.XlsX.FromXLSX(moduleName, "en", xlsx);			
		}
	}
}
