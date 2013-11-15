using System;
using System.Linq;

using System.Collections.Generic;
using System.Xml.Serialization;

using TranslationTool.IO;
using Google.Apis.Drive.v2.Data;

namespace TranslationTool
{
    class Program
    {
		static string directory = @"D:\Serveurs\Sites\iLucca\iLuccaEntities\Resources\";
		static string testDir = @"D:\Users\login\Documents\i18n\XlsxTest";
        static void Main(string[] args)
        {
			//SyncWithoutKey2();
			//SyncWithoutKeyAnnulerUneDemande();
			//Console.ReadLine();
			//return;

			SyncGoogle("Cleemy-SEPA", "WEXPENSESSEPA"); return;
			//SyncGoogle("WFIGGO_BUGS", "WFIGGO");
			//SyncGoogle("2812 - traduction Figgo Evolutions ouvrables", "WFIGGOOuvrables");
			//SyncGoogle("Cleemy - Invités Externes", "WEXPENSESExternalAttendees");
			//return;
			var folder = IO.Google.Drive.FindFolder("Translation System");
			

			//UploadAllToGdocs(folder);

			var spreadsheets = IO.Google.Drive.FindSpreadsheetFiles(folder);
			var wfiggoXlsx = IO.Google.Drive.DownloadFile(spreadsheets.Single(ss => ss.Title == "WFIGGO"));
			return;
			var wfiggo = IO.XlsX.FromXLSX("WFIGGO", "en", wfiggoXlsx);


			SyncWithoutKey();

			foreach (var project in new string[] { "WFIGGO", "FIACCUEIL", "FICOMMON" })
			{ 
				var revision1 = CSV.FromCSV(@"D:\Users\login\Downloads\_WFIGGO_FIACCUEIL_FICOMMON - Traductions (2).csv", project, "en");
				var revision2 = CSV.FromCSV(@"D:\Users\login\Downloads\_WFIGGO_FIACCUEIL_FICOMMON - Traductions (3).csv", project, "en");

				var toPatch = ResX.FromResX(directory, project, "en");
				var diff = revision1.Diff(revision2);

				toPatch.Patch(diff);

				ResX.ToResX(toPatch, directory);
				diff.Print();
			}
        }

		static void UploadAllToGdocs(File folder)
		{
			var t = IO.Collection.ResX.FromResX(directory, "en");
			//TranslationTool.IO.Google.Tests.DriveTests.Upload(t.Projects["FIPLANNING"]);

			foreach (var file in IO.Collection.Xls.ToDir(t, testDir))
			{
				//I know this looks stupid, but Google Docs would'nt accept our generated XlsX files. Hence we generate Xls files and convert
				//them to XlsX files (using LibreOffice) as a workaround... 
				string file2 = IO.Xls.ToXlsX(file.Key);
				IO.Google.Drive.UploadXlsx(file.Value.Project, file2, folder);
			}
			return;
		}

		static string SyncWithoutKeyFileAnnuler = @"D:\Users\login\Downloads\Figgo - traductions - Annuler une demande.csv";
		static string SyncWithoutKeyFileImport = @"D:\Users\login\Downloads\Figgo - traductions - Import des salariés.csv";
		static string SyncWithoutKeyFileLogin = @"D:\Users\login\Downloads\trad login - Traduction.csv";

		static void SyncWithoutKeyAnnulerUneDemande()
		{
			var matchBase = ResX.FromResX(directory, "MailUser", "en");
			//var withOutKeys = CSV.FromCSV(SyncWithoutKeyFileImport, "WFIGGO", "en", false);
			var withOutKeys = CSV.FromCSV(SyncWithoutKeyFileLogin, "MailUser", "en", true);
			
			var keyMatcher = new Memory.KeyMatcher(matchBase, withOutKeys, "fr");
			keyMatcher.ChangeKeys(withOutKeys);
			withOutKeys.RemoveKeys(keyMatcher.NoMatches);

			var diff = matchBase.Diff(withOutKeys);
			//diff.DiffPerLanguage["fr"].Print("fr");
			diff.DiffPerLanguage["fr"].PrintDiff();

			matchBase.Patch(diff);
			ResX.ToResX(matchBase, directory);			
		}


		static string SyncWithoutKeyFile = @"D:\Users\login\Downloads\Figgo - traductions - Actions à réaliser with keys.xlsx";
		static string SyncPlanning = @"D:\Users\login\Documents\i18n\Figgo_NL\Traduction Figgo Planning_ITA complet.xlsx";
		static void SyncWithoutKey2()
		{
			string project = "FIPLANNING";
			var matchBase = ResX.FromResX(directory, project, "en");
			var withKeys = XlsX.FromXLSX(project, "en", SyncPlanning);

			var diff = matchBase.Diff(withKeys);
			//diff.DiffPerLanguage["fr"].Print("fr");
			diff.DiffPerLanguage["fr"].PrintDiff();

			matchBase.Patch(diff);
			ResX.ToResX(matchBase, directory);
		}


		static void SyncWithoutKey()
		{
			var testCSVWithoutKey = CSV.FromCSV(@"D:\Users\login\Downloads\Figgo - traductions - Actions à réaliser.csv", "WFIGGO", "en");
			var matchBase = ResX.FromResX(directory, "WFIGGO", "en");

			var keyMatcher = new Memory.KeyMatcher(matchBase, testCSVWithoutKey, "fr");
			keyMatcher.ChangeKeys(testCSVWithoutKey);

			testCSVWithoutKey.RenameVariables("[variable \"nom du compteur\"]", "[C]");
			testCSVWithoutKey.RenameVariables("[variable \"nom du user\"]", "[U]");
			testCSVWithoutKey.RenameVariables("[variable \"nombre de jours\"]", "[N]");
			testCSVWithoutKey.RenameVariables("[variable compte]", "[C]");
			testCSVWithoutKey.RenameVariables("[variable nombre de comptes]", "[NBRE]");
			testCSVWithoutKey.RenameVariables("[1]", "[N]");
			testCSVWithoutKey.RenameVariables("[xxx]", "[NumeroCompte]");


			//CSV.ToCSV(testCSVWithoutKey, @"D:\Users\login\Downloads\");
			XlsX.ToXLSX(testCSVWithoutKey, SyncWithoutKeyFile);
			//XlsX.ToXLSX(matchBase, @"D:\Users\login\Downloads\Figgo - traductions - Actions à réaliser origkeys.xlsx");
			var diff = matchBase.Diff(testCSVWithoutKey);
			//diff.Print();
			diff.DiffPerLanguage["fr"].Print("fr");
		}

		static void SyncGoogle(string gdocsTitle, string resxName)
		{
			var tpGoogle = GDataSpreadSheet.FromGDoc(gdocsTitle);

			var tpCurrent = ResX.FromResX(directory, resxName, "en");
			tpGoogle.Project = tpCurrent.Project;
			var allSync = tpCurrent.SyncWith(tpGoogle);

			//tpCurrent.RemoveEmptyKeys();
			ResX.ToResX(tpCurrent, directory);
		}

		public static void SyncProject(string project, string csvFile, string targetDir)
		{
			var tpCurrent = ResX.FromResX(directory, project, "en");
			var tpNew = CSV.FromCSV(csvFile, project, "en");
			//tp.ToResX(targetDir);
			var allSync = tpCurrent.SyncWith(tpNew);

			tpCurrent.RemoveEmptyKeys();
			ResX.ToResX(tpCurrent, targetDir);

			var sw = new System.IO.StreamWriter(targetDir + @"\" + project + "resume.txt", false);
			//            allSync["en"].Print("en", sw);
		}

		public static void PatchFromTwoRevision(TranslationModule r1, TranslationModule r2, TranslationModule toPatch)
		{
			var diff = r1.Diff(r2);
			toPatch.Patch(diff);
		}
    }    
}