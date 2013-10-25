using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslationTool.Memory
{
	/// <summary>
	/// Matches message resource keys in two translation projects based on message similarity.
	/// </summary>
	public class KeyMatcher
	{
		public Dictionary<string, string> Matches { get; protected set; }

		public KeyMatcher(TranslationProject tpBase, TranslationProject tp, string masterLanguage = null)
		{
			var memory = new TranslationMemory(tpBase);
			Matches = new Dictionary<string,string>();

			masterLanguage = masterLanguage ?? tp.MasterLanguage;

			foreach (var seg in tp.Dicts[masterLanguage])
			{
				var results = memory.Query(masterLanguage, seg.Value);

				if(results.Any())
				{
					Matches.Add(seg.Key, results.First().Key);
				}				
			}
		}

		/// <summary>
		/// Based on the matched keys, exchanges keys in the given translaton project
		/// </summary>
		/// <param name="tp"></param>
		public void ChangeKeys(TranslationProject tp)
		{
			foreach(var kvp in Matches)
			{
				if(kvp.Key == kvp.Value) continue;

				foreach(var lang in tp.Languages)
				{
					if(tp.Dicts[lang].ContainsKey(kvp.Key) && !tp.Dicts[lang].ContainsKey(kvp.Value))
					{
						tp.Dicts[lang].Add(kvp.Value, tp.Dicts[lang][kvp.Key]);
						tp.Dicts[lang].Remove(kvp.Key);
					} else if(!tp.Dicts[lang].ContainsKey(kvp.Key))
					{
						tp.Dicts[lang].Add(kvp.Value, "");
					}
				}				
			}
		}

	}
}
