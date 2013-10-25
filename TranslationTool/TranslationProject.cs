using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TranslationTool
{
	public class Segment
	{
		public string Language { get; protected set; }
		public string Text { get; set; }

		public Segment(string lang, string t)
		{
			this.Language = lang;
			this.Text = t;
		}
	}

	public class SegmentSet
	{
		public string Key { get; protected set; }
		public IEnumerable<Segment> Segments { get; protected set; }

		public SegmentSet(string key, IEnumerable<Segment> segments)
		{
			this.Key = key;
			this.Segments = segments;
		}
	}

    public class TranslationProject
    {
        public Dictionary<string, Dictionary<string, string>> Dicts;
		public Dictionary<string, string> Comments;

        public IEnumerable<string> Languages;
		public string MasterLanguage { get; protected set; }
		public string Project { get; set; }
		

		public IEnumerable<string> Keys
		{
			get
			{
				return Dicts[MasterLanguage].Keys;
			}
		}

        public TranslationProject(string project, string masterLanguage) : this(project, masterLanguage, new string[] { "en", "de", "es", "fr", "it", "nl" })
        {
        }

        public TranslationProject(string project, string masterLanguage, string[] languages)
        {
			this.MasterLanguage = masterLanguage;
            this.Languages = languages;
            this.Dicts = new Dictionary<string, Dictionary<string, string>>();            
            this.Project = project;
			this.Comments = new Dictionary<string, string>();
        }
                     

		public IEnumerable<SegmentSet> Segments
		{
			get
			{
				List<SegmentSet> sets = new List<SegmentSet>();

				foreach (var key in Keys)
				{
					List<Segment> Segments = new List<Segment>();
					foreach (var kvp in Dicts.Where(kvp => kvp.Value.ContainsKey(key)))
					{
						Segments.Add(new Segment(kvp.Key, kvp.Value[key]));
					}

					//yield return new SegmentSet(key, Dicts.Where(kvp => kvp.Value.ContainsKey(key)).Select(kvp => new Segment(kvp.Key, kvp.Value[key])));
					sets.Add(new SegmentSet(key, Segments));	
				}
				return sets;
			}
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
                if(!Dicts.ContainsKey(l)) continue;
                

                List<string> keysToRemove = new List<string>();

                foreach (var kvp in Dicts[l])
                    if (string.IsNullOrWhiteSpace(kvp.Value.Trim()))
                        keysToRemove.Add(kvp.Key);

                foreach (var key in keysToRemove)
                    Dicts[l].Remove(key);
            }
        }

        public Dictionary<string, DictDiff> SyncWith(TranslationProject tp)
        {
            var allSync = new Dictionary<string, DictDiff>();

            foreach (var l in Languages)
            {
                if (!Dicts.ContainsKey(l) || !tp.Dicts.ContainsKey(l)) continue;
                allSync.Add(l, DictDiff.SyncDicts(Dicts[l], tp.Dicts[l]));
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
    }
}
