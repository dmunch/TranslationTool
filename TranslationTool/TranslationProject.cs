﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

	public class TranslationProjectBase
	{
		public IEnumerable<string> Languages;
		public string MasterLanguage { get; protected set; }
		public string Project { get; set; }

		public TranslationProjectBase(TranslationProjectBase other)
		{
			this.MasterLanguage = other.MasterLanguage;
			this.Languages = other.Languages;
			this.Project = other.Project;
		}

		public TranslationProjectBase(string project, string masterLanguage)
			: this(project, masterLanguage, new string[] { "en", "de", "es", "fr", "it", "nl" })
		{
		}

		public TranslationProjectBase(string project, string masterLanguage, string[] languages)
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

	public class TranslationProjectDiff : TranslationProjectBase
	{
		public Dictionary<string, DictDiff> DiffPerLanguage { get; protected set; }

		public TranslationProjectDiff(TranslationProjectBase other, Dictionary<string, DictDiff> _diffPerLanguage) : base(other)
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

    public class TranslationProject : TranslationProjectBase
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

        public TranslationProject(string project, string masterLanguage) : base(project, masterLanguage)
        {
			this.Dicts = new Dictionary<string, Dictionary<string, string>>();
			this.Comments = new Dictionary<string, string>();
        }

        public TranslationProject(string project, string masterLanguage, string[] languages) : base(project, masterLanguage, languages)
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

        public TranslationProjectDiff SyncWith(TranslationProject tp)
        {
			var diff = Diff(tp);
			Patch(diff);

			return diff;
		}

		public TranslationProjectDiff Diff(TranslationProject tp)
		{
			var allSync = new Dictionary<string, DictDiff>();

			foreach (var l in Languages)
			{
				if (!Dicts.ContainsKey(l) || !tp.Dicts.ContainsKey(l)) continue;
				allSync.Add(l, DictDiff.Diff(Dicts[l], tp.Dicts[l]));				
			}

			return new TranslationProjectDiff(this, allSync);
		}

		public void Patch(TranslationProjectDiff tpDiff)
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
