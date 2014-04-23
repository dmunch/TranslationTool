using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace TranslationTool.Helpers
{
	public class GeneralDiff<T, TKey, TContent>
	{
		public Dictionary<TKey, T> Orig = new Dictionary<TKey, T>();
		public Dictionary<TKey, T> Updated = new Dictionary<TKey, T>();
		public Dictionary<TKey, T> New = new Dictionary<TKey, T>();
		public Dictionary<TKey, T> Deleted = new Dictionary<TKey, T>();

		protected Func<T, TKey> KeySelector;
		protected Func<T, TContent> ContentSelector;
		protected Action<T, TContent> ContentSetter;
		
		protected Func<TContent, TContent, bool> IsModifiedComparer;

		public GeneralDiff(Expression<Func<T, TKey>> _KeySelector, Expression<Func<T, TContent>> contentSelector)
		{
			this.KeySelector = _KeySelector.Compile();
			this.ContentSelector = contentSelector.Compile();

			var newValue = Expression.Parameter(contentSelector.Body.Type);
			var assign = Expression.Lambda<Action<T, TContent>>(
								Expression.Assign(contentSelector.Body, newValue),
								contentSelector.Parameters[0], newValue);

			ContentSetter = assign.Compile();
		}


		public virtual void Diff(IEnumerable<T> list1, IEnumerable<T> list2)
		{
			var dDiff = new DictDiff();

			this.Orig.Clear();
			this.Updated.Clear();
			this.New.Clear();
			this.Deleted.Clear();

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
				if (dict2.ContainsKey(kvp.Key) && IsModifiedComparer(ContentSelector(kvp.Value), ContentSelector(dict2[kvp.Key])))
				//if (dict2.ContainsKey(kvp.Key) && !ContentSelector(kvp.Value).Equals(ContentSelector(dict2[kvp.Key])))
				{
					Updated.Add(kvp.Key, dict2[kvp.Key]);
					Orig.Add(kvp.Key, kvp.Value);
				}
			}

			//check new keys  
			foreach (var kvp in dict2)
			{
				if (!dict1.ContainsKey(kvp.Key))
				{
					New.Add(kvp.Key, kvp.Value);
				}
			}

			//check deleted keys  
			foreach (var kvp in dict1)
			{
				if (!dict2.ContainsKey(kvp.Key))
				{
					Deleted.Add(kvp.Key, kvp.Value);
				}
			}
		}


		//public void Print(TextWriter os)
		public void Print(ILogging logging)
		{
			if (logging == null)
				logging = new ConsoleLogging();

			logging.WriteLine("Updated {0} rows.", Updated.Count);
			foreach (var kvp in Updated)
			{
				logging.WriteLine("K: {0}\n Old: {1} \n New: {2}", KeySelector(kvp.Value), ContentSelector(Orig[kvp.Key]), ContentSelector(kvp.Value));
			}

			logging.WriteLine("Added {0} rows.", New.Count);
			foreach (var kvp in New)
			{
				logging.WriteLine("{0}", kvp.Key);
			}
		}

		public void Patch(IList<T> listToPatch, IEnumerable<T> lookup)
		{
			this.Patch(listToPatch, lookup.ToDictionary(KeySelector));
		}

		public void Patch(IList<T> listToPatch)
		{
			this.Patch(listToPatch, listToPatch);
		}

		public void Patch(IList<T> listToPatch, IDictionary<TKey, T> lookup)
		{
			var diff = this;

			foreach (var kvp in diff.New)
			{
				if (!lookup.ContainsKey(kvp.Key))
				{
					listToPatch.Add(kvp.Value);
				}
			}

			foreach (var kvp in diff.Updated)
			{
				
				ContentSetter(lookup[kvp.Key], ContentSelector(kvp.Value));
			}
		}
	}
}
