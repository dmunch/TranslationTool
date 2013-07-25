using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Collections;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace TranslationTool
{
    public class TranslationProject
    {
        Dictionary<string, string> masterDict;
        Dictionary<string, Dictionary<string, string>> dicts;

        protected IEnumerable<string> Languages;
        protected string masterLanguage;
        protected string project;
        
        public TranslationProject(string project)
        {
            Languages = new string[] { "de", "es", "fr", "it", "nl" };
            dicts = new Dictionary<string, Dictionary<string, string>>();
            masterLanguage = "en";
        
            this.project = project;
        }

        public static TranslationProject FromCSV(string file, string project)
        {
            // open the file "data.csv" which is a CSV file with headers
            var dicts = new Dictionary<string, Dictionary<string, string>>();


            using (CsvReader csv =
                   new CsvReader(new StreamReader(file), true))
            {
                int fieldCount = csv.FieldCount;
                string currentNS = "";

                string[] headers = csv.GetFieldHeaders();
                for (int c = 1; c < headers.Length; c++)
                    dicts.Add(headers[c].ToLower(), new Dictionary<string, string>());

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

            var tp = new TranslationProject(project);
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

            sb.Append("").Append("en").Append(";");
            foreach (var l in Languages)
            {
                sb.Append(l);
                sb.Append(";");
            }
            sb.AppendLine();

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
            using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + project + ".csv", false, Encoding.UTF8))
            {
                outfile.Write(sb.ToString());
            }
        }

        public void Print()
        {

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
                if (d2.ContainsKey(kvp.Key) && kvp.Value != d2[kvp.Key])
                {
                    toSync.Updated.Add(kvp.Key, d2[kvp.Key]);
                    toSync.Orig.Add(kvp.Key, kvp.Value);
                }
            }
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
