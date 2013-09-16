using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Collections;
using System.IO;
using LumenWorks.Framework.IO.Csv;
using System.Xml.Serialization;
using System.Xml;

namespace TranslationTool
{
    public class TranslationProject
    {
        Dictionary<string, string> masterDict;
        Dictionary<string, Dictionary<string, string>> dicts;

        public IEnumerable<string> Languages;
        public string masterLanguage;
        protected string project;
        
        public TranslationProject(string project) : this(project, new string[] { "de", "es", "fr", "it", "nl" })
        {
        }

        public TranslationProject(string project, string[] languages)
        {
            this.Languages = languages;
            dicts = new Dictionary<string, Dictionary<string, string>>();
            masterLanguage = "en";

            this.project = project;
        }
      
        public static TranslationProject FromCSV(string file, string project)
        {
            // open the file "data.csv" which is a CSV file with headers
            var dicts = new Dictionary<string, Dictionary<string, string>>();
            List<string> languages = new List<string>();

            using (CsvReader csv =
                   new CsvReader(new StreamReader(file), true))
            {
                int fieldCount = csv.FieldCount;
                string currentNS = "";

                string[] headers = csv.GetFieldHeaders();

                for (int c = 1; c < headers.Length; c++)
                {
                    string language = headers[c].ToLower();
                    dicts.Add(language, new Dictionary<string, string>());
                    languages.Add(language);
                }

                while (csv.ReadNextRecord())
                {
                    string key = csv[0];
                    if(key.Contains("ns:"))
                        currentNS = key.Split(':')[1];

                    if(currentNS == project && !key.Contains("ns:"))
                        if(!string.IsNullOrWhiteSpace(key))
                            for (int i = 1; i < fieldCount; i++)
                                dicts[headers[i].ToLower()].Add(key, csv[i]);
                }

            }

            languages.Remove("en");
            var tp = new TranslationProject(project, languages.ToArray());
            tp.masterDict = dicts[tp.masterLanguage];
            dicts.Remove(tp.masterLanguage);
            tp.dicts = dicts;

            return tp;
        }

        public static TranslationProject FromResX(string dir, string project)
        {
            var tp = new TranslationProject(project);
            tp.Load(dir, project);

            return tp;
        }

        protected void Load(string directory, string project)
        {
            masterDict = GetDictFromResX(directory + project + ".resx");

            foreach (var l in Languages)
            {
                var file = directory + project + "." + l + ".resx";
                dicts.Add(l, GetDictFromResX(file));
            }
        }

        static Dictionary<string, string> GetDictFromResX(string fileName)
        {
            // Enumerate the resources in the file.
            //ResXResourceReader rr = ResXResourceReader.FromFileContents(file);
            ResXResourceReader rr = new ResXResourceReader(fileName);
            IDictionaryEnumerator dict = rr.GetEnumerator();

            var stringDict = new Dictionary<string, string>();
            while (dict.MoveNext())
                stringDict.Add(dict.Key as string, dict.Value as string);

            return stringDict;
        }

        public void ToResX(string targetDir)
        {
            ToResX(masterDict, targetDir + project + ".resx");
            foreach (var l in Languages)
            {
                Dictionary<string, string> dict;
                if (dicts.ContainsKey(l))
                    dict = dicts[l];
                else
                    dict = EmptyFromTemplate(masterDict);

                ToResX(dict, targetDir + project + "." + l + ".resx");
            }
        }

        protected static Dictionary<string, string> EmptyFromTemplate(Dictionary<string, string> template)
        {
            var dict = new Dictionary<string, string>();
            foreach (var kvp in template)
            {
                dict.Add(kvp.Key, "");
            }

            return dict;
        }

        protected static void ToResX(Dictionary<string, string> dict, string fileName)
        {
            using (ResXResourceWriter resx = new ResXResourceWriter(fileName))
            {
                foreach (var kvp in dict)
                    resx.AddResource(kvp.Key, kvp.Value);
            }
        }

        public void ToCSV(string targetDir)
        {
            StringBuilder sb = new StringBuilder();
            ToCSV(sb, true);

            using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + project + ".csv", false, Encoding.UTF8))
            {
                outfile.Write(sb.ToString());
            }
        }

        public void ToCSV(StringBuilder sb, bool addHeader = true)
        {
            if (addHeader)
            {
                sb.Append("").Append("en").Append(";");
                foreach (var l in Languages)
                {
                    sb.Append(l);
                    sb.Append(";");
                }
                sb.AppendLine();
            }
            foreach (var kvp in masterDict)
            {
                sb.Append(kvp.Key).Append(";");
                foreach (var l in Languages)
                {
                    sb.Append("'");
                    sb.Append(dicts[l].ContainsKey(kvp.Key) ? dicts[l][kvp.Key] : "");
                    sb.Append("';");
                }

                sb.AppendLine();
            }
        }

        public void ToXLS(string fileName)
        {
            FileInfo newFile = new FileInfo(fileName);
            if (newFile.Exists)
            {
                newFile.Delete();  // ensures we create a new workbook
                newFile = new FileInfo(fileName);
            }
            using (var package = new OfficeOpenXml.ExcelPackage(newFile))
            {
                var worksheet = package.Workbook.Worksheets.Add("Traductions");
                ToXLS(worksheet, 1);

                package.Save();
            }
        }

        public int ToXLS(OfficeOpenXml.ExcelWorksheet worksheet, int rowStart = 1)
        {
            int columnCounter = 1;
            int rowCounter = rowStart;

            if (rowStart == 1) //write header
            {
                worksheet.Cells[rowStart, columnCounter++].Value = "";
                worksheet.Cells[rowStart, columnCounter++].Value = masterLanguage;
                foreach (var l in Languages)
                    worksheet.Cells[rowStart, columnCounter++].Value = l;
                rowCounter++;
            }

            foreach (var kvp in masterDict)
            {                
                columnCounter = 1;
                worksheet.Cells[rowCounter, columnCounter++].Value = kvp.Key;
                worksheet.Cells[rowCounter, columnCounter++].Value = kvp.Value; //write master language
                foreach (var l in Languages)
                    worksheet.Cells[rowCounter, columnCounter++].Value = dicts.ContainsKey(l) ? dicts[l].ContainsKey(kvp.Key) ? dicts[l][kvp.Key] : "" : "";
                rowCounter++;
            }

            return rowCounter;
        }

        public void ToArb(string targetDir)
        {
            StringBuilder sb = new StringBuilder();
            
            sb = ToArb(targetDir, project, masterLanguage, masterDict);
            foreach(var l in Languages)
                if(dicts.ContainsKey(l))
                    sb = ToArb(targetDir, project, l, dicts[l]);
            /*
             using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + project + ".arb", false, Encoding.UTF8))
            {
                outfile.Write(sb.ToString());
            }*/
        }
        public void ToArbAll(string targetDir)
        {
            StringBuilder sb = new StringBuilder();

            sb = ToArb(sb, project, masterLanguage, masterDict);
            foreach (var l in Languages)
                if (dicts.ContainsKey(l))
                    sb = ToArb(sb, project, l, dicts[l]);
            
             using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + project + ".arb", false, Encoding.UTF8))
            {
                outfile.Write(sb.ToString());
            }
        }
        public StringBuilder ToArb(string targetDir, string project, string language, Dictionary<string, string> dict)
        {
            StringBuilder sb = new StringBuilder();

            sb = ToArb(sb, project, language, dict);
            using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + project + "." + language + ".arb", false, Encoding.UTF8))
            {
                outfile.Write(sb.ToString());
            }

            return sb;
        }
        
        public static StringBuilder ToArb(StringBuilder sb, string project, string language, Dictionary<string, string> dict)
        {
            string newLine = "";
            sb.Append("arb.register(\"arb_ref_app\",{").Append(newLine);
            sb.Append("\"@@locale\":\"").Append(language).Append("\",").Append(newLine);
            sb.Append("\"@@context\":\"").Append(project).Append("\",").Append(newLine);        

            foreach(var kvp in dict)
            {
                sb.Append("\"").Append(kvp.Key).Append("\":\"").Append(kvp.Value).Append("\",").Append(newLine);
            }
            sb.Remove(sb.Length - 1, 1).Append(newLine); //remove trailing ,
            sb.Append("});").Append(newLine).Append(newLine);

            return sb;
        }

        public void toTMX(string targetDir)
        {
            var tmx = new tmx();
            var tus = new List<tu>();
          
            foreach (var kvp in masterDict)
            {
                var tu = new tu();
                tu.tuid = kvp.Key;

                var tuvs = new List<tuv>();
                tuvs.Add(new tuv() { lang = masterLanguage.ToUpper(), lang1 = masterLanguage.ToUpper(), seg = new seg() { Text = new string[] { kvp.Value } } });

                foreach (var l in Languages)
                {                         
                    if(dicts.ContainsKey(l) && dicts[l].ContainsKey(kvp.Key))
                        tuvs.Add(new tuv() { lang = l.ToUpper(), lang1 = l.ToUpper(), seg = new seg() { Text = new string[] { dicts[l][kvp.Key] } } });
                }

                tu.tuv = tuvs.ToArray();
                tus.Add(tu);
            }
            
            tmx.body = tus.ToArray();
            tmx.header = new header() { srclang = "*all*", creationtool = "iLuccaTranslationTool", segtype = headerSegtype.phrase };

            var extraTypes = new Type[] { typeof(tu), typeof(tuv), typeof(seg) }; 
            XmlSerializer xs = new XmlSerializer(typeof(tmx), null, extraTypes, new XmlRootAttribute("tmx"), null);

			//specify encoding to add BOM
            using (var sw = new StreamWriter(targetDir + @"\" + project + ".tmx", false, new UTF8Encoding(false)))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = false;
                settings.NewLineHandling = NewLineHandling.None;
                //xs.Serialize(sw, tmx);

                using (XmlWriter writer = XmlWriter.Create(sw, settings))
                {
                    xs.Serialize(writer, tmx);
                    //_serializer.Serialize(o, writer);
                }
            }
        }
        
        public void Print()
        {

        }
        
        public void RemoveEmptyKeys()
        {
            foreach (var l in Languages)
            {
                if(!dicts.ContainsKey(l)) continue;
                

                List<string> keysToRemove = new List<string>();

                foreach (var kvp in dicts[l])
                    if (string.IsNullOrWhiteSpace(kvp.Value.Trim()))
                        keysToRemove.Add(kvp.Key);

                foreach (var key in keysToRemove)
                    dicts[l].Remove(key);
            }
        }

        public Dictionary<string, Sync> SyncWith(TranslationProject tp)
        {
            var allSync = new Dictionary<string, Sync>();

            allSync.Add(masterLanguage, SyncDicts(masterDict, tp.masterDict));
            allSync[masterLanguage].Print(masterLanguage);

            foreach (var l in Languages)
            {
                if (!dicts.ContainsKey(l) || !tp.dicts.ContainsKey(l)) continue;
                allSync.Add(l, SyncDicts(dicts[l], tp.dicts[l]));
                allSync[l].Print(l);
            }
            return allSync;
        }

        public static void PrintSynced(Dictionary<string, string> synced, string language)
        {
            Console.WriteLine("Synced {0} rows in {1}.", synced.Count, language);
            foreach (var kvp in synced)
                Console.WriteLine(kvp.Key);

        }
        public static Sync SyncDicts(Dictionary<string, string> d1, Dictionary<string, string> d2)
        {
           // List<string> synced = new List<string>();
            var toSync = new Sync();

            
            foreach (var kvp in d1)
            {
                if (d2.ContainsKey(kvp.Key) && kvp.Value != d2[kvp.Key] && d2[kvp.Key].Trim() != "")
                {
                    toSync.Updated.Add(kvp.Key, d2[kvp.Key]);
                    toSync.Orig.Add(kvp.Key, kvp.Value);
                }
            }

            //don't add new keys            
            foreach (var kvp in d2)
            {
                if (!d1.ContainsKey(kvp.Key))
                {
                    d1.Add(kvp.Key, kvp.Value);
                    toSync.New.Add(kvp.Key, kvp.Value);
                }
            }
            
            
            foreach (var kvp in toSync.Updated)
                d1[kvp.Key] = kvp.Value;
                                            
            return toSync;
        }

        public class Sync
        {
            public Dictionary<string, string> Orig = new Dictionary<string, string>();
            public Dictionary<string, string> Updated = new Dictionary<string, string>();
            public Dictionary<string, string> New = new Dictionary<string, string>();

            public Sync()
            {
            }

            public void Print(string language, TextWriter os = null)
            {
                if (os == null)
                    os = Console.Out;

                os.WriteLine("Updated {0} rows in {1}.", Updated.Count, language);
                foreach (var kvp in Updated)

                    os.WriteLine("K: {0}: Old: {1} | New {2}", kvp.Key, Orig[kvp.Key], kvp.Value);

                os.WriteLine("Added {0} rows in {1}.", New.Count, language);
                foreach (var kvp in New)
                    os.WriteLine(kvp.Key);
            }
        }
    }
}
