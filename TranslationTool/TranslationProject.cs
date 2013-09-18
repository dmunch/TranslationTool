using System;
using System.Collections.Generic;
using System.IO;

namespace TranslationTool
{
    public class TranslationProject
    {
        public Dictionary<string, string> masterDict;
        public Dictionary<string, Dictionary<string, string>> dicts;

        public IEnumerable<string> Languages;
        public string masterLanguage;
        public string project;
        
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
                     
        public static Dictionary<string, string> EmptyFromTemplate(Dictionary<string, string> template)
        {
            var dict = new Dictionary<string, string>();
            foreach (var kvp in template)
            {
                dict.Add(kvp.Key, "");
            }

            return dict;
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
