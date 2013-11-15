using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TranslationTool.Helpers;

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

	public class TranslationModuleBase
	{
		public IEnumerable<string> Languages;
		public string MasterLanguage { get; protected set; }
		public string Project { get; set; }

		public TranslationModuleBase(TranslationModuleBase other)
		{
			this.MasterLanguage = other.MasterLanguage;
			this.Languages = other.Languages;
			this.Project = other.Project;
		}

		public TranslationModuleBase(string project, string masterLanguage)
			: this(project, masterLanguage, new string[] { "en", "de", "es", "fr", "it", "nl" })
		{
		}

		public TranslationModuleBase(string project, string masterLanguage, string[] languages)
		{
			this.MasterLanguage = masterLanguage;
			this.Languages = languages;
			
			this.Project = project;
		}
			
		public static void PrintSynced(Dictionary<string, string> synced, string language)
		{
			Console.WriteLine("Synced {0} rows in {1}.", synced.Count, language);
			foreach (var kvp in synced)
				Console.WriteLine(kvp.Key);

		}		
	}

	public class TranslationModuleDiff : TranslationModuleBase
	{
		public Dictionary<string, DictDiff> DiffPerLanguage { get; protected set; }

		public TranslationModuleDiff(TranslationModuleBase other, Dictionary<string, DictDiff> _diffPerLanguage) : base(other)
		{
			this.DiffPerLanguage = _diffPerLanguage;
		}

		public void Print()
		{
			foreach (var diff in DiffPerLanguage)
			{
				diff.Value.Print(diff.Key);
			}
		}
	}

    public class TranslationModule : TranslationModuleBase
    {
        public Dictionary<string, Dictionary<string, string>> Dicts;
		public Dictionary<string, string> Comments;
		
		public IEnumerable<string> Keys
		{
			get
			{
				return Dicts[MasterLanguage].Keys;
			}
		}

        public TranslationModule(string project, string masterLanguage) : base(project, masterLanguage)
        {
			this.Dicts = new Dictionary<string, Dictionary<string, string>>();
			this.Comments = new Dictionary<string, string>();
        }

        public TranslationModule(string project, string masterLanguage, string[] languages) : base(project, masterLanguage, languages)
        {
            this.Dicts = new Dictionary<string, Dictionary<string, string>>();            
			this.Comments = new Dictionary<string, string>();

			foreach (var lang in Languages)
			{
				this.Dicts.Add(lang, new Dictionary<string, string>());
			}
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

		public void RenameVariables(string old, string newName)
		{
			foreach (var l in Languages)
			{
				if (!Dicts.ContainsKey(l)) continue;

				Dictionary<string, string> keysToReplace = new Dictionary<string, string>();
				foreach (var kvp in Dicts[l])
				{
					if (string.IsNullOrWhiteSpace(kvp.Value.Trim())) continue;
					keysToReplace.Add(kvp.Key, kvp.Value.Replace(old, newName));
				}

				foreach (var kvp in keysToReplace)
				{
					Dicts[l][kvp.Key] = kvp.Value;
				}
			}
		}

		public void RemoveKeys(IEnumerable<string> keys)
		{
			foreach (var l in Languages)
			{
				if (!Dicts.ContainsKey(l)) continue;
				
				var dict = Dicts[l];
				foreach (var key in keys)
				{
					if (dict.ContainsKey(key))
					{
						dict.Remove(key);
					}
				}
			}
		}

        public TranslationModuleDiff SyncWith(TranslationModule tp)
        {
			var diff = Diff(tp);
			Patch(diff);

			return diff;
		}

		public TranslationModuleDiff Diff(TranslationModule tp)
		{
			var allSync = new Dictionary<string, DictDiff>();

			foreach (var l in Languages)
			{
				if (!Dicts.ContainsKey(l) || !tp.Dicts.ContainsKey(l)) continue;
				allSync.Add(l, DictDiff.Diff(Dicts[l], tp.Dicts[l]));				
			}

			return new TranslationModuleDiff(this, allSync);
		}

		public void Patch(TranslationModuleDiff tpDiff)
		{
			foreach (var l in Languages)
			{
				if (!Dicts.ContainsKey(l) || !tpDiff.DiffPerLanguage.ContainsKey(l)) continue;
				DictDiff.Patch(Dicts[l], tpDiff.DiffPerLanguage[l]);				
			}
		}

		/// <summary>
		/// Proposes a resource key based on a translation message
		/// </summary>
		/// <param name="sentence"></param>
		/// <returns></returns>
		public string KeyProposal(string sentence)
		{
			var words = sentence.Split(' ');
			StringBuilder keyBuilder = new StringBuilder();
			int wordCount = 0;
			while (keyBuilder.Length / 2 < Math.Min(words.Length, 3))
			{
				var word = words[wordCount++].ToUpper();
				if (word.Contains("[") || word.Contains("]") || word.Contains("{") || word.Contains("}")) continue;

				keyBuilder.Append(word);
				keyBuilder.Append('_');
			}
			string keyBase = keyBuilder.ToString().Replace(' ', '_').TrimEnd(' ', '_');
			string key = keyBase;
			int keyCounter = 1;
			while (Dicts[MasterLanguage].ContainsKey(key))
			{
				key = keyBase + "_" + (keyCounter++).ToString();
			}

			return key;
		}

        public static void PrintSynced(Dictionary<string, string> synced, string language)
        {
            Console.WriteLine("Synced {0} rows in {1}.", synced.Count, language);
            foreach (var kvp in synced)
                Console.WriteLine(kvp.Key);

        }       
    }
}
