using System;

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
            //var tp = TranslationProject.FromCSV(csvFile, project);
         
            SyncProject("WFIGGOMAIL");
            SyncProject("WFIGGO");
         
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
