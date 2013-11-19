using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TranslationTool.Helpers;

namespace TranslationTool
{
	public static class ExtensionMethods
	{
		/*
		public static ILookup<string, ILookup<string, T>> ToDoubleLookup<T>(this IEnumerable<T> list, Expression<Func<T, string>> key1Selector, Expression<Func<T, string>> key2Selector)
		{
			Func<T, string> _CompiledFunc1 = key1Selector.Compile();
			Func<T, string> _CompiledFunc = key2Selector.Compile();
			return list.ToLookup(item => item, new Comparer2<T, string>(key1Selector))
																  .ToLookup(s => _CompiledFunc(s.Key), s => s.ToLookup(item => _CompiledFunc1(item)));
		}*/

		public static ILookup<string, Segment> ByLanguage(this IEnumerable<Segment> segments)
		{
			return segments.ToLookup(s => s.Language);
		}
		public static ILookup<string, Segment> ByKey(this IEnumerable<Segment> segments)
		{
			return segments.ToLookup(s => s.Key);
		}

		/// <summary>
		/// Proposes a resource key based on a translation message
		/// </summary>
		/// <param name="sentence"></param>
		/// <returns></returns>
		public static string KeyProposal(this ITranslationModule module, string sentence)
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

			while (module.ContainsKey(key))
			{
				key = keyBase + "_" + (keyCounter++).ToString();
			}

			return key;
		}
	}
	
	public class TranslationModule : TranslationModuleBase, TranslationTool.ITranslationModule
	{
		public IEnumerable<Segment> Segments { get { return _Segments; } }

		protected List<Segment> _Segments { get; set; }

		public IEnumerable<string> Keys
		{
			get
			{
				return Segments.Select(s => s.Key);
			}
		}

		public TranslationModule(string project, string masterLanguage)
			: base(project, masterLanguage)
		{
			this._Segments = new List<Segment>();
		}

		public TranslationModule(string project, string masterLanguage, string[] languages)
			: base(project, masterLanguage, languages)
		{
			this._Segments = new List<Segment>();
		}

		public void Add(Segment s)
		{
			this._Segments.Add(s);
		}

		public void Add(IEnumerable<Segment> s)
		{
			this._Segments.AddRange(s);
		}

		public ILookup<string, Segment> ByKey
		{
			get
			{
				return Segments.ByKey();
			}
		}

		public ILookup<string, Segment> ByLanguage
		{
			get
			{
				return Segments.ByLanguage();
			}
		}

		public void RemoveKeys(IEnumerable<string> keys)
		{
			var byKeys = this.Segments.ByKey();
			foreach (var key in keys.Where(k => byKeys.Contains(k)))
			{
				Remove(byKeys[key]);
			}			
		}
		
		public bool ContainsKey(string key)
		{
			return this.Segments.FirstOrDefault(s => s.Key == key) != null;
		}

		public void Remove(IEnumerable<Segment> segments)
		{
			foreach (var s in segments.ToList())
				this._Segments.Remove(s);
		}

		public void RemoveEmptyKeys()
		{
			Remove(Segments.Where(s => string.IsNullOrWhiteSpace(s.Text)));
		}

		public void RenameVariables(string old, string newName)
		{
			foreach (var s in Segments.Where(s => !string.IsNullOrWhiteSpace(s.Text)))
			{
				s.Text = s.Text.Replace(old, newName);
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
			var allSync = new Dictionary<string, SegmentDiff>();

			
			foreach (var l in Segments.ByLanguage())
			{
				//if (!Dicts.ContainsKey(l) || !tp.Dicts.ContainsKey(l)) continue;
				var segDiff = new SegmentDiff();
				segDiff.Diff(l, tp.ByLanguage[l.Key]);
				allSync.Add(l.Key, segDiff);
			}

			/*
			foreach (var l in Languages)
			{
				if (!Dicts.ContainsKey(l) || !tp.Dicts.ContainsKey(l)) continue;
				allSync.Add(l, DictDiff.Diff(Dicts[l], tp.Dicts[l]));
			}
			*/
			return new TranslationModuleDiff(this, allSync);
		}

		public void Patch(TranslationModuleDiff tpDiff)
		{
			var byLanguage = Segments.ByLanguage();
			foreach (var diff in tpDiff.DiffPerLanguage)
			{
				diff.Value.Patch(_Segments, byLanguage[diff.Key]);
			}
		}
		
		public static void PrintSynced(Dictionary<string, string> synced, string language)
		{
			Console.WriteLine("Synced {0} rows in {1}.", synced.Count, language);
			foreach (var kvp in synced)
				Console.WriteLine(kvp.Key);

		}
	}
}
