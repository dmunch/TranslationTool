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
		
        public IEnumerable<string> Languages;
		public string MasterLanguage { get; protected set; }
		public string Project { get; set; }
		public Dictionary<string, string> Comments;

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

        public Dictionary<string, Sync> SyncWith(TranslationProject tp)
        {
            var allSync = new Dictionary<string, Sync>();

            foreach (var l in Languages)
            {
                if (!Dicts.ContainsKey(l) || !tp.Dicts.ContainsKey(l)) continue;
                allSync.Add(l, SyncDicts(Dicts[l], tp.Dicts[l]));
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
			var diff = Diff(d1, d2);
			Patch(d1, diff);

			return diff;
        }

		public static void Patch(Dictionary<string, string> d, Sync toSync)
		{
			foreach (var kvp in toSync.New)
			{
				if (!d.ContainsKey(kvp.Key))
				{
					d.Add(kvp.Key, kvp.Value);
				}
			}

			foreach (var kvp in toSync.Updated)
			{ 
				d[kvp.Key] = kvp.Value;
			}
		}

		public static Sync Diff(Dictionary<string, string> d1, Dictionary<string, string> d2)
		{
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
					toSync.New.Add(kvp.Key, kvp.Value);
				}
			}

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
