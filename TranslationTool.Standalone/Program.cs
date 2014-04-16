﻿using System;
using System.Linq;

using CommandLine;
using CommandLine.Text;

using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;

namespace TranslationTool.Standalone
{
	class Options
	{
		[Option('r', "resxdir", DefaultValue = @"D:\Serveurs\Sites\iLucca\iLuccaEntities\Resources\",
		  HelpText = "Location of ResX files")]
		public string ResXDir { get; set; }

		[Option('g', "gdrive", Required = false,
		  HelpText = "Name of Google Drive folder for restricted search")]
		public string GDriveFolder { get; set; }

		[Option('f', "file", Required = false, MutuallyExclusiveSet = "folder",
		  HelpText = "Name of file to download")]
		public string FileName { get; set; }


		[Option('n', "multiSpreadsheet",
		  HelpText = "Use the names of the spreadsheets in the file as the module name. Otherwise only the first spreadsheet is processed.")]
		public bool MultiSpreadsheet { get; set; }

		[Option('m', "moduleName",
		  HelpText = "Name of the RESX file. Used for upload or in case only the first spreadsheet is processed for download. If option -n is used only export this specific module.")]
		public string ModuleName { get; set; }

		[Option('u', "upload",
			HelpText = "Upload to Gdoc instead of downloading.")]
		public bool Upload { get; set;}

		[Option('s', "soffice",
			HelpText = "Path to local SOffice installation used for upload.",
			DefaultValue = @"C:\Program Files (x86)\LibreOffice 4.0\program\soffice.exe")]
		public string SOfficePath { get; set; }

		/*
		[Option('f', "file", MutuallyExclusiveSet = "file",
		  HelpText = "Name of module to download")]
		public string ModuleName { get; set; }
		*/

		[Option('v', "verbose", Required = false,
		  HelpText = "BeVerbose")]
		public bool Verbose { get; set; }

		[Option('a', "angular", Required = false, DefaultValue = false, HelpText = "Output files in Angular translate format instead of RESX")]
		public bool Angular { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this,
			  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}

	class Program
	{
		static string testDir = @"D:\Users\login\Documents\i18n\XlsxTest";
		static string resxDir = @"D:\Serveurs\Sites\iLucca\iLuccaEntities\Resources\";
		
		static void Main(string[] args)
		{
			var options = new Options();
			
			if (CommandLine.Parser.Default.ParseArguments(args, options))
			{
				var main = new Main(options);
				main.Run();
			}
		
		}
					
		static void UploadAllToGdocs(string directory, File folder)
		{
			var t = IO.Collection.ResX.FromResX(directory, "en");
			//TranslationTool.IO.Google.Tests.DriveTests.Upload(t.Projects["FIPLANNING"]);

			foreach (var file in IO.Collection.XlsCollection.ToDir(t, testDir))
			{
				Console.Write("Processing {0} ...", file.Value.Name);
				//I know this looks stupid, but Google Docs would'nt accept our generated XlsX files. Hence we generate Xls files and convert
				//them to XlsX files (using LibreOffice) as a workaround... 
				string file2 = IO.Xls.ToXlsX(file.Key);
				IO.Google.Drive.UploadXlsx(file.Value.Name, file2, folder);

				Console.WriteLine("Done");
			}
			return;
		}		
	}

	class Main
	{
		Options options;
		public Main(Options options)
		{
			this.options = options;
		}

		public void Run()
		{
			//var drive = new IO.Google.Drive2(IO.Google.Drive.GetServiceAccountService());
			var drive = new IO.Google.Drive2(IO.Google.Drive.GetUserCredential());

			if (options.Upload)
			{
				Upload(options, drive);
			}
			else
			{
				Download(options, drive);
			}
		}
		
		protected void Upload(Options options, IO.Google.Drive2 drive)
		{
			if (string.IsNullOrWhiteSpace(options.ModuleName))
			{
				Console.WriteLine("No module name specified, batch upload all modules found in {0}", options.ResXDir);
				UploadBatch(options, drive);

				return;
			}

			Console.Write("Uploading {0} ...", options.ModuleName);

			var folder = drive.FindFolder(options.GDriveFolder);

			var t = IO.ResX.FromResX(options.ResXDir, options.ModuleName, "en");
			
			string tempPath = System.IO.Path.GetTempPath();
			string xlsFilename = System.IO.Path.Combine(tempPath, options.ModuleName + ".xls");
			IO.Xls.ToXLS(t, xlsFilename);
								
			//I know this looks stupid, but Google Docs would'nt accept our generated XlsX files. 
			//Hence we generate Xls files and convert them to XlsX files (using LibreOffice) as a workaround... 			
			string xlsx = IO.Xls.ToXlsX(xlsFilename, options.SOfficePath);
			IO.Google.Drive.UploadXlsx(options.ModuleName, xlsx, folder);

			Console.WriteLine("Done");			
		}

		protected void UploadBatch(Options options, IO.Google.Drive2 drive)
		{
			var folder = drive.FindFolder(options.GDriveFolder);

			var t = IO.Collection.ResX.FromResX(options.ResXDir, "en");			
			string tempPath = System.IO.Path.GetTempPath();

			foreach (var file in IO.Collection.XlsCollection.ToDir(t, tempPath))
			{
				Console.Write("Uploading {0} ...", file.Value.Name);
		
				//I know this looks stupid, but Google Docs would'nt accept our generated XlsX files. 
				//Hence we generate Xls files and convert them to XlsX files (using LibreOffice) as a workaround... 			
				string file2 = IO.Xls.ToXlsX(file.Key, options.SOfficePath);
				IO.Google.Drive.UploadXlsx(file.Value.Name, file2, folder);

				Console.WriteLine("Done");
			}
			return;
		}

		protected void Download(Options options, IO.Google.Drive2 drive)
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

			var xlsx = drive.DownloadFile(spreadsheet, true);

			if (!options.MultiSpreadsheet)
			{
				var project = IO.XlsX.FromXLSX(options.FileName, "en", xlsx);
				project.Name = options.ModuleName;

				ToOutputFormat(project);
			}
			else
			{
				var projects = IO.Collection.XlsX.FromMultiSpreadsheet("en", xlsx);

				if (options.ModuleName == null)
				{
					//If no moduleName is specified, export all sheets
					foreach (var project in projects.Projects.Where(p => !p.Key.StartsWith("_")))
					{
						ToOutputFormat(project.Value);
					}
				}
				else
				{
					//A module was specified, only export this one
					var project = projects.Projects.Where(p => p.Key == options.ModuleName).Single();
					ToOutputFormat(project.Value);
				}
			}
		}

		protected void ToOutputFormat(TranslationModule module)
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
