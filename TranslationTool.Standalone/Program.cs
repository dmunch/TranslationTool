using System;
using System.Linq;

using CommandLine;
using CommandLine.Text;

using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using TranslationTool.Helpers;
using System.Collections.Generic;

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
		public string GDriveFolder { get; set; }	
	}

	class BaseDownloadOptions : BaseOptions
	{
		[Option('n', "multiSpreadsheet", HelpText = "Use the names of the spreadsheets in the file as the module name. " +
													 "Otherwise only the first spreadsheet is processed.")]
		public bool MultiSpreadsheet { get; set; }

		[Option('m', "moduleName",
		  HelpText = "Name of the RESX file. Used in case only the first spreadsheet is processed for download. " +
					 "If option -n is used only export this specific module.")]
		public string ModuleName { get; set; }

		[OptionList('t', "tags",
			HelpText = "Filter by tags. Tags are given in gdoc file by a special line before a block, starting with the hash-bang: #js",
			DefaultValue = null)]
		public IList<string> Tags { get; set; }

 		[Option('f', "file",
			Required = true,
			HelpText = "Name of file to download")]
		public string FileName { get; set; }

		[Option('g', "gdrive",
			Required = false,
			HelpText = "Name of Google Drive folder for restricted search")]
		public string GDriveFolder { get; set; }
		
	}

	class DownloadOptions : BaseDownloadOptions
	{		
		[Option('a', "angular", 
			Required = false, 
			DefaultValue = false,
			HelpText = "Output files in Angular translate format instead of RESX")]
		public bool Angular { get; set; }
	}

	class DiffOptions : BaseDownloadOptions
	{
		[Option("html", Required = false, DefaultValue = false,
			HelpText = "show diff as formatted HTML (opens in browser)")]
		public bool HTML { get; set; }
	}

	class PatchOptions : BaseOptions
	{
		[Option('m', "moduleName",
			Required = true,
			HelpText = "Name of the RESX file. E.g. FIGGO in case of FIGGO.en.resx")]
		public string ModuleName { get; set; }
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

		[VerbOption("diff", HelpText = "show difference between local file and online gdoc")]
		public DiffOptions DiffVerb { get; set; }

		[VerbOption("patch", HelpText = "patch local files with patch file from stdin")]
		public PatchOptions PatchVerb { get; set; }


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
			
			CommandLine.Parser parser = new CommandLine.Parser(new CommandLine.ParserSettings {
							
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
					case "diff":
						{
							var diffOptions = subOptions as DiffOptions;
							var drive = new IO.Google.Drive2(IO.Google.Drive.GetUserCredential());

							var folder = drive.FindFolder(diffOptions.GDriveFolder);
							ITranslationModule localModule = IO.ResX.FromResX(diffOptions.ResXDir, diffOptions.ModuleName, "en");

							var downloader = new Downloader(drive);
							var xlsx = downloader.DownloadXlsx(diffOptions);

							var _remoteModule = IO.XlsX.FromXLSX(diffOptions.FileName, "en", xlsx);
							_remoteModule.Name = diffOptions.ModuleName;

							ITranslationModule remoteModule = _remoteModule.FilterByTags(diffOptions.Tags);

							TranslationModuleDiff diff = localModule.Diff(remoteModule);

							var diffJson = Newtonsoft.Json.JsonConvert.SerializeObject(diff, Newtonsoft.Json.Formatting.Indented);
							Console.OutputEncoding = System.Text.Encoding.UTF8;
							Console.Write(diffJson);

							if(diffOptions.HTML)
							{
								var htmlLogger = new HTMLConcatLogger();
								diff.Print(htmlLogger);
								var fileName = System.IO.Path.GetTempFileName() + ".html";
								System.IO.File.WriteAllText(fileName, htmlLogger.HTML);
								System.Diagnostics.Process.Start(fileName);
							}
						}
						break;
					case "patch":
						{
							var patchOptions = subOptions as PatchOptions;

							//read patch data from stdin
							Console.InputEncoding = System.Text.Encoding.UTF8;							
							var jsonReader = new System.Text.StringBuilder();
							string s;
							while ((s = Console.ReadLine()) != null)
							{
								jsonReader.AppendLine(s);
							}
							string json = jsonReader.ToString();

							//string json = System.IO.File.ReadAllText(@".\diff");							
							var diff = Newtonsoft.Json.JsonConvert.DeserializeObject<TranslationModuleDiff>(json);

							TranslationModule localModule = IO.ResX.FromResX(patchOptions.ResXDir, patchOptions.ModuleName, "en");
							localModule.Patch(diff);

							IO.ResX.ToResX(localModule, patchOptions.ResXDir);

						}
						break;
				}
			}

			return 0;
		}				
	}	
}
