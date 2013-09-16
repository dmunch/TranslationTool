using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace TranslationTool
{
    class Program
    {
        static string targetDir = @"D:\Users\login\Documents\i18n\";
        static string directory = @"D:\Serveurs\Sites\iLucca\iLuccaEntities\Resources\";
        static string csvFileGdocs = @"D:\Users\login\Downloads\Figgo - traductions - Annuler une demande.csv";

        static string allCsvFileGdocs = @"D:\Users\login\Downloads\_WFIGGO_FIACCUEIL_FICOMMON - Traductions.csv";

        static void Main(string[] args)
        {
            Cleemy();
            return;

            //TMXTest(@"..\..\TMX14\test.tmx");

            //string csvFile = @"D:\Users\login\Documents\i18n\Figgo - traductions.csv";
            var tp = TranslationProject.FromCSV(csvFileGdocs, "WFIGGO");
            tp.ToXLS(targetDir + @"\WFIGGO.xlsx");
            tp.ToArb(targetDir);
            tp.ToArbAll(targetDir);
            tp.toTMX(targetDir);

            //SyncProject("WFIGGOMAIL");
            SyncProject("WFIGGO");

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

        static void Cleemy()
        {
            string fileGDocs = @"D:\Users\login\Downloads\Cleemy - Invités Externes - Sheet1.csv";
            var tp = TranslationProject.FromCSV(fileGDocs, "WEXPENSESExternalAttendees");
            tp.RemoveEmptyKeys();
            tp.ToResX(directory);
        }

        static void SyncProject(string project)
        {
            var tpCurrent = TranslationProject.FromResX(directory, project);
            var tpNew = TranslationProject.FromCSV(csvFileGdocs, project);
            //tp.ToResX(targetDir);
            var allSync = tpCurrent.SyncWith(tpNew);

            tpCurrent.RemoveEmptyKeys();
            tpCurrent.ToResX(targetDir);

            var sw = new System.IO.StreamWriter(targetDir + @"\" + project + "resume.txt", false);
            allSync["en"].Print("en", sw);
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

            var tuv1 = new tuv() { lang = "de"};
            tuv1.seg = new seg(){Text = new string[]{"hello"}};

            var tuv2 = new tuv() { lang = "en" };
            tuv2.seg = new seg() { Text = new string[] { "hello" } };

            //mytu.Items = new tuv[] { tuv1, tuv2 };
            mytu.tuv = new tuv[] { tuv1, tuv2 };
            mytu.tuid = "HELLO";
            
            myTmx.body = new tu[]{mytu};
            myTmx.header = new header() { srclang = "fr", creationtool = "iLucca", segtype = headerSegtype.sentence };
            using(var sw = new StreamWriter(fileName + "x"))
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