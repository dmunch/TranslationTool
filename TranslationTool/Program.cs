using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

using TranslationTool.IO;

namespace TranslationTool
{
    class Program
    {
		static string directory = @"D:\Serveurs\Sites\iLucca\iLuccaEntities\Resources\";

        static void Main(string[] args)
        {
			string password = "";
			SyncGoogle("dmunch@lucca.fr", password, "WFIGGO_BUGS", "WFIGGO");
			//SyncGoogle("dmunch@lucca.fr", password, "Cleemy - Invités Externes", "WEXPENSESExternalAttendees");	
        }

		static void SyncGoogle(string userName, string password, string gdocsTitle, string resxName)
		{
			var tpGoogle = GDataSpreadSheet.FromGDoc(userName, password, gdocsTitle);


			var tpCurrent = ResX.FromResX(directory, resxName, "en");
			tpGoogle.Project = tpCurrent.Project;
			var allSync = tpCurrent.SyncWith(tpGoogle);

			tpCurrent.RemoveEmptyKeys();
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
    }    
}