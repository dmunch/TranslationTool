using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Linq.Expressions;

namespace TranslationTool.Helpers
{
	public class StringContentDiff<T, TKey> :  GeneralDiff<T, TKey, string>
	{
		public StringContentDiff(Func<T, TKey> KeySelector, Func<T, string> contentSelector) 
			:base(KeySelector, contentSelector)
		{
		
		}

		public void PrintDiff(TextWriter os = null)
		{
			if (os == null)
				os = Console.Out;

			var diff = new DiffMatchPatch.diff_match_patch();
			foreach (var kvp in Updated)
			{
				var diffs = diff.diff_main(ContentSelector(Orig[kvp.Key]), ContentSelector(kvp.Value));
				diff.diff_cleanupSemantic(diffs);
				os.WriteLine("K: {0}, diffs {1}", kvp.Key, diffs.Count);

				foreach (var d in diffs)
				{
					os.WriteLine("{0}: {1}", d.operation, d.text);
				}
				//diff.diff_prettyHtml(diffs);
			}
		}
	}

	public class GeneralDiff<T, TKey, TContent>
	{
		public Dictionary<TKey, T> Orig = new Dictionary<TKey, T>();
		public Dictionary<TKey, T> Updated = new Dictionary<TKey, T>();
		public Dictionary<TKey, T> New = new Dictionary<TKey, T>();

		protected Func<T, TKey> KeySelector;
		public Func<T, TContent> ContentSelector;
		protected IEqualityComparer<TContent> EqualityComparer;

		public GeneralDiff(Func<T, TKey> KeySelector, Func<T, TContent> contentSelector)
		{
			this.KeySelector = KeySelector;
			this.ContentSelector = contentSelector;
		}


		public void Diff(IEnumerable<T> list1, IEnumerable<T> list2)
		{
			//Diff(new LambdaComparer<T, TKey>(KeySelector));
			IEqualityComparer<T> equalityComparer = null;
			var dDiff = new DictDiff();
			
			this.Orig.Clear();
			this.Updated.Clear();
			this.New.Clear();

			var dict1 = list1.ToDictionary(KeySelector);
			var dict2 = list2.ToDictionary(KeySelector);

			/*
			if (d1 == null)
			{
				foreach (var kvp in d2)
				{
					toSync.New.Add(kvp.Key, kvp.Value);				
				}
			}
			*/


			foreach (var kvp in dict1)
			{
				//if (dict2.ContainsKey(kvp.Key) && equalityComparer.Equals(kvp.Value, dict2[kvp.Key]))
				if (dict2.ContainsKey(kvp.Key) && ContentSelector(kvp.Value).Equals(ContentSelector(dict2[kvp.Key])))
				{
					Updated.Add(kvp.Key, dict2[kvp.Key]);
					Orig.Add(kvp.Key, kvp.Value);
				}
			}

			//check new keys  
			foreach (var kvp in dict1)
			{
				if (!dict1.ContainsKey(kvp.Key))
				{
					New.Add(kvp.Key, kvp.Value);
				}
			}			
		}

		

		public void Print(TextWriter os = null)
		{
			if (os == null)
				os = Console.Out;

			os.WriteLine("Updated {0} rows.", Updated.Count);
			foreach (var kvp in Updated)
			{
				os.WriteLine("K: {0}\n Old: {1} \n New: {2}", KeySelector(kvp.Value), ContentSelector(Orig[kvp.Key]), ContentSelector(kvp.Value));
			}

			os.WriteLine("Added {0} rows.", New.Count);
			foreach (var kvp in New)
			{
				os.WriteLine(kvp.Key);
			}
		}

		public void Patch(IDictionary<TKey, T> d)
		{
			 GeneralDiff<T, TKey, TContent>.Patch(d, this);
		}

		public void Patch(IEnumerable<T> d)
		{
			Patch(d.ToDictionary(KeySelector));
		}

		public static void Patch(IDictionary<TKey, T> d, GeneralDiff<T, TKey, TContent> toSync)
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
	}

	public class DictDiff
	{
		public Dictionary<string, string> Orig = new Dictionary<string, string>();
		public Dictionary<string, string> Updated = new Dictionary<string, string>();
		public Dictionary<string, string> New = new Dictionary<string, string>();

		public DictDiff()
		{
		}

		public void PrintDiff(TextWriter os = null)
		{
			if (os == null)
				os = Console.Out;
		
			var diff = new DiffMatchPatch.diff_match_patch();
			foreach (var kvp in Updated)
			{
				var diffs = diff.diff_main(Orig[kvp.Key], kvp.Value);
				diff.diff_cleanupSemantic(diffs);
				os.WriteLine("K: {0}, diffs {1}", kvp.Key, diffs.Count);

				foreach (var d in diffs)
				{
					os.WriteLine("{0}: {1}", d.operation, d.text);
				}
				//diff.diff_prettyHtml(diffs);
			}
		}

		public void Print(string language, TextWriter os = null)
		{
			if (os == null)
				os = Console.Out;

			os.WriteLine("Updated {0} rows in {1}.", Updated.Count, language);
			foreach (var kvp in Updated)
			{ 
				os.WriteLine("K: {0}\n Old: {1} \n New: {2}", kvp.Key, Orig[kvp.Key], kvp.Value);
			}

			os.WriteLine("Added {0} rows in {1}.", New.Count, language);
			foreach (var kvp in New)
			{ 
				os.WriteLine(kvp.Key);
			}
		}

		public static DictDiff SyncDicts(Dictionary<string, string> d1, Dictionary<string, string> d2)
		{
			var diff = Diff(d1, d2);
			Patch(d1, diff);

			return diff;
		}

		public static void Patch(Dictionary<string, string> d, DictDiff toSync)
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


		public static DictDiff Diff(Dictionary<string, string> d1, Dictionary<string, string> d2)
		{
			var toSync = new DictDiff();

			/*
			if (d1 == null)
			{
				foreach (var kvp in d2)
				{
					toSync.New.Add(kvp.Key, kvp.Value);				
				}
			}
			*/
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
	}
}
