using System;
using System.Collections.Generic;

namespace TranslationTool
{
    class Program
    {
        static string targetDir = @"D:\Users\login\Documents\i18n\";
        static string directory = @"D:\Serveurs\Sites\iLucca\iLuccaEntities\Resources\";
        static string csvFileGdocs = @"D:\Users\login\Downloads\Figgo - traductions - Annuler une demande.csv";

        static void Main(string[] args)
        {       
            //string csvFile = @"D:\Users\login\Documents\i18n\Figgo - traductions.csv";
            var tp = TranslationProject.FromCSV(csvFileGdocs, "WFIGGO");
            tp.ToXLS(targetDir + @"\WFIGGO.xlsx");

            SyncProject("WFIGGOMAIL");
            //SyncProject("WFIGGO");

            var pc = new TranslationProjectCollection(new string[] { "WFIGGO", "FIACCUEIL", "FICOMMON" }, directory);
            //pc.ToCSV(targetDir);
            pc.ToXLS(targetDir);

            Console.WriteLine("Finished");
            Console.ReadLine();
        }

        static void SyncProject(string project)
        {
            var tpCurrent = TranslationProject.FromResX(directory, project);
            var tpNew = TranslationProject.FromCSV(csvFileGdocs, project);
            //tp.ToResX(targetDir);
            var allSync = tpCurrent.SyncWith(tpNew);

            tpCurrent.ToResX(targetDir);

            var sw = new System.IO.StreamWriter(targetDir + @"\" + project + "resume.txt", false);
            allSync["en"].Print("en", sw);
        }
    }

    
}
