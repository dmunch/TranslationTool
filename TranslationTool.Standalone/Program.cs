using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Linq;

using System.Collections.Generic;
using System.Xml.Serialization;

using TranslationTool.IO;
using Google.Apis.Drive.v2.Data;
using CommandLine;
using CommandLine.Text;

namespace TranslationTool.Standalone
{
	class Program
	{
		static string testDir = @"D:\Users\login\Documents\i18n\XlsxTest";
		static string resxDir = @"D:\Serveurs\Sites\iLucca\iLuccaEntities\Resources\";

		class Options
		{
			[Option('r', "resxdir", DefaultValue= @"D:\Serveurs\Sites\iLucca\iLuccaEntities\Resources\", 
			  HelpText = "Location of ResX files")]
			public string ResXDir { get; set; }

			[Option('g', "gdrive", Required = false,
			  HelpText = "Name of Google Drive folder for restricted search")]
			public string GDriveFolder { get; set; }

			[Option('f', "file", Required=true, MutuallyExclusiveSet="folder",
			  HelpText = "Name of file to download")]
			public string FileName { get; set; }


			[Option('n', "multiSpreadsheet", 
			  HelpText = "Use the names of the spreadsheets in the file as the module name. Otherwise only one spreadsheet is accepted which has to be called 'Translations'")]
			public bool MultiSpreadsheet { get; set; }

			/*
			[Option('f', "file", MutuallyExclusiveSet = "file",
			  HelpText = "Name of module to download")]
			public string ModuleName { get; set; }
			*/

			[Option('v', "verbose", Required = false,
			  HelpText = "BeVerbose")]
			public bool Verbose { get; set; }

			[HelpOption]
			public string GetUsage()
			{
				return HelpText.AutoBuild(this,
				  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
			}
		}


		static void Main(string[] args)
		{
			var options = new Options();
			
			if (CommandLine.Parser.Default.ParseArguments(args, options))
			{		
				File spreadsheet = null;
				if (options.GDriveFolder != null)
				{
					var folder = TranslationTool.IO.Google.Drive.FindFolder(options.GDriveFolder);
					var spreadsheets = IO.Google.Drive.FindSpreadsheetFiles(folder);

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
					spreadsheet = TranslationTool.IO.Google.Drive.FindSpreadsheetFile(options.FileName);
				}

				if (spreadsheet == null)
				{
					Console.WriteLine("Module {0} not found. Returning.", options.FileName);
				}
				
				var xlsx = IO.Google.Drive.DownloadFile(spreadsheet, true);

				if(!options.MultiSpreadsheet)
				{
					var project = IO.XlsX.FromXLSX(options.FileName, "en", xlsx);
					IO.ResX.ToResX(project, options.ResXDir);
				}
				else
				{
					var projects = IO.Collection.XlsX.FromMultiSpreadsheet("en", xlsx);

					foreach (var project in projects.Projects)
					{
						IO.ResX.ToResX(project.Value, options.ResXDir);
					}
				}
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
}
