using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

using TranslationTool.IO;

namespace TranslationTool
{
	class Tests
	{
		static string directory = @"D:\Serveurs\Sites\iLucca\iLuccaEntities\Resources\";
		static string csvFileGdocs = @"D:\Users\login\Downloads\Figgo - traductions - Annuler une demande.csv";
		static string targetDir = @"D:\Users\login\Documents\i18n\";
		static string allCsvFileGdocs = @"D:\Users\login\Downloads\_WFIGGO_FIACCUEIL_FICOMMON - Traductions.csv";
		static string figgoBugsCsv = @"D:\Users\login\Downloads\WFIGGO_BUGS - Traductions.csv";

		public static void TestDivers()
		{
			Tests.TranslationMemory();


			//Cleemy();
			//return;

			//TMXTest(@"..\..\TMX14\test.tmx");
			//SyncProject("WFIGGOMAIL");
			//SyncProject("WFIGGO", csvFileGdocs, targetDir);


			//SyncProject("WFIGGO", figgoBugsCsv, directory);

			Program.SyncProject("WFIGGO", allCsvFileGdocs, directory);
			//SyncProject("FIACCUEIL", allCsvFileGdocs, directory);
			Program.SyncProject("FICOMMON", allCsvFileGdocs, directory);

			/*
			SyncProject("WFIGGO", csvFileGdocs, directory);			
			SyncProject("WFIGGOMAIL", csvFileGdocs, directory);
			*/
			return;

			//string csvFile = @"D:\Users\login\Documents\i18n\Figgo - traductions.csv";
			var tp = CSV.FromCSV(csvFileGdocs, "WFIGGO", "en");
			XlsX.ToXLSX(tp, targetDir + @"\WFIGGO.xlsx");
			Arb.ToArb(tp, targetDir);
			Arb.ToArbAll(tp, targetDir);
			TMX.ToTMX(tp, targetDir);


			/*
			var pc = TranslationProjectCollection.FromResX(new string[] { "WFIGGO", "FIACCUEIL", "FICOMMON" }, directory);
			//pc.ToCSV(targetDir);
			//pc.ToXLS(targetDir);

			var pcCsv = TranslationProjectCollection.FromCSV(new string[] { "WFIGGO", "FIACCUEIL", "FICOMMON" }, allCsvFileGdocs);
			pc.SyncWith(pcCsv);
			pc.ToResX(targetDir);
			*/

			Console.WriteLine("Finished");
			Console.ReadLine();
		}


		public static void TranslationMemory()
		{
			var tp = CSV.FromCSV(csvFileGdocs, "WFIGGO", "en");
			var tpc = IO.Collection.ResX.FromResX(new string[] { "WFIGGO", "FIACCUEIL", "FICOMMON" }, directory, "en");
			
			var memory = new Memory.TranslationMemory(tpc);
			memory.Query("fr", "annuller demande");
		}

		static void Cleemy()
		{
			string fileGDocs = @"D:\Users\login\Downloads\Cleemy - Invités Externes - Sheet1.csv";
			var tp = CSV.FromCSV(fileGDocs, "WEXPENSESExternalAttendees", "en");
			tp.RemoveEmptyKeys();
			ResX.ToResX(tp, directory);
		}
		
		static void TMXTest(string fileName)
		{
			XmlRootAttribute xRoot = new XmlRootAttribute();
			xRoot.ElementName = "tmx";
			// xRoot.Namespace = "http://www.cpandl.com";
			xRoot.IsNullable = true;

			//XmlSerializer xs = new XmlSerializer(typeof(tmx), xRoot);
			var extraTypes = new Type[] { typeof(tu), typeof(tuv), typeof(seg) };
			XmlSerializer xs = new XmlSerializer(typeof(tmx), null, extraTypes, new XmlRootAttribute("tmx"), null);
			//var xs = new XmlSerializer(typeof(tmx), extraTypes);
			/*
			var tmx = new tmx();
			XmlSerializer serializer = new XmlSerializer(tmx.GetType());            
			object deserialized = serializer.Deserialize(reader.BaseStream);
			tmx = (tmx)deserialized;
			*/
			StreamReader reader = new StreamReader(fileName);
			var tmx = (tmx)xs.Deserialize(reader.BaseStream);


			var myTmx = new tmx();

			var mytu = new tu();

			var tuv1 = new tuv() { lang = "de" };
			tuv1.seg = new seg() { Text = new string[] { "hello" } };

			var tuv2 = new tuv() { lang = "en" };
			tuv2.seg = new seg() { Text = new string[] { "hello" } };

			//mytu.Items = new tuv[] { tuv1, tuv2 };
			mytu.tuv = new tuv[] { tuv1, tuv2 };
			mytu.tuid = "HELLO";

			myTmx.body = new tu[] { mytu };
			myTmx.header = new header() { srclang = "fr", creationtool = "iLucca", segtype = headerSegtype.sentence };
			using (var sw = new StreamWriter(fileName + "x"))
			{
				xs.Serialize(sw, myTmx);
			}

			using (var r = new StreamReader(fileName + "x"))
			{
				tmx = (tmx)xs.Deserialize(r.BaseStream);
			}
		}
	}
}
