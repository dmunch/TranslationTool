using System;
using System.Linq;

using CommandLine;
using CommandLine.Text;

using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;

namespace TranslationTool.Standalone
{
	abstract class BaseOptions
	{
		[Option('r', "resxdir", DefaultValue = @"D:\Serveurs\Sites\iLucca\iLuccaEntities\Resources\", HelpText = "Location of ResX files")]
		public string ResXDir { get; set; }
		
		[Option('v', "verbose", Required = false, HelpText = "Be verbose")]
		public bool Verbose { get; set; }
	}

	class UploadOptions : BaseOptions
	{
		[Option('s', "soffice",
			HelpText = "Path to local SOffice installation used for upload.",
			DefaultValue = @"C:\Program Files (x86)\LibreOffice 4.0\program\soffice.exe")]
		public string SOfficePath { get; set; }

		[Option('b', "batch", 					
			DefaultValue=false,
			MutuallyExclusiveSet = "files")]
		public bool Batch { get; set; }

		[Option('m', "moduleName",			
			HelpText = "Name of the RESX file. E.g. FIGGO in case of FIGGO.en.resx",
			MutuallyExclusiveSet = "files")]
		public string ModuleName { get; set; }

		[Option('g', "gdrive",
			Required = true,
			HelpText = "Name of Google Drive folder for upload")]
		public string GDriveFolder { get; set; }	}

	class DownloadOptions : BaseOptions
	{
		[Option('n', "multiSpreadsheet",  HelpText = "Use the names of the spreadsheets in the file as the module name. " +
													 "Otherwise only the first spreadsheet is processed.")]
		public bool MultiSpreadsheet { get; set; }

		[Option('m', "moduleName",
		  HelpText = "Name of the RESX file. Used in case only the first spreadsheet is processed for download. " +
					 "If option -n is used only export this specific module.")]
		public string ModuleName { get; set; }

		[Option('f', "file", 
			Required = true,
			HelpText = "Name of file to download")]
		public string FileName { get; set; }

		[Option('g', "gdrive",
			Required = false,
			HelpText = "Name of Google Drive folder for restricted search")]
		public string GDriveFolder { get; set; }

		[Option('a', "angular", 
			Required = false, 
			DefaultValue = false,
			HelpText = "Output files in Angular translate format instead of RESX")]
		public bool Angular { get; set; }
	}

	class Options
	{				
		public Options()
		{
			DownloadVerb = new DownloadOptions();
			UploadVerb = new UploadOptions();
		}

		[VerbOption("upload", HelpText="upload translation files")]
		public UploadOptions UploadVerb { get; set; }

		[VerbOption("download", HelpText="download translation files")]
		public DownloadOptions DownloadVerb { get; set; }

		[HelpVerbOption]
		public string GetUsage(string verb)
		{
			return HelpText.AutoBuild(this, verb);
		}
	}

	class Program
	{	
		static int Main(string[] args)
		{
			var options = new Options();
			string verb = "";

			object subOptions = null;
			var ps = new CommandLine.ParserSettings();
			
			CommandLine.Parser parser = new CommandLine.Parser(() => return new CommandLine.ParserSettings {
							
                            MutuallyExclusive = true,
                            CaseSensitive = false,
                            HelpWriter = Console.Error});

			parser = CommandLine.Parser.Default;
			
			if (parser.ParseArguments(args, options, (_verb, _subOptions) =>
				{
					verb = _verb;
					subOptions = _subOptions;
				}))
			{
				switch (verb)
				{
					case "upload":
						{
							var uploadOptions = subOptions as UploadOptions;

							if (!uploadOptions.Batch && string.IsNullOrWhiteSpace(uploadOptions.ModuleName)) 
							{
								Console.Error.Write(options.GetUsage("upload"));
								return -1;
							}

							var drive = new IO.Google.Drive2(IO.Google.Drive.GetUserCredential());
							var uploader = new Uploader(drive);

							uploader.Upload(uploadOptions);
						}
						break;
					case "download":
						{
							var drive = new IO.Google.Drive2(IO.Google.Drive.GetUserCredential());
							var downloader = new Downloader(drive);

							downloader.Download(subOptions as DownloadOptions);
						}
						break;
				}
			}

			return 0;
		}				
	}	
}
