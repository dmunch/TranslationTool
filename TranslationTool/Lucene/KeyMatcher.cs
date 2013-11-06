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
		public List<string> NoMatches { get; protected set; }
		public KeyMatcher(TranslationProject tpBase, TranslationProject tp, string masterLanguage = null)
		{
			var memory = new TranslationMemory(tpBase);
			Matches = new Dictionary<string,string>();
			NoMatches = new List<string>();

			masterLanguage = masterLanguage ?? tp.MasterLanguage;

			foreach (var seg in tp.Dicts[masterLanguage])
			{
				var results = memory.Query(masterLanguage, seg.Value);

				if (results.Any())
				{
					Matches.Add(seg.Key, results.First().Key);
				}
				else
				{
					NoMatches.Add(seg.Key);
				}
			}
		}

		/// <summary>
		/// Based on the matched keys, exchanges keys in the given translaton project
		/// </summary>
		/// <param name="tp"></param>
		public void ChangeKeys(TranslationProject tp)
		{
			int counter = 0;
			foreach(var kvp in Matches)
			{
				if(kvp.Key == kvp.Value) continue;

				foreach(var langKey in tp.Languages)
				{
					var lang = tp.Dicts[langKey];

					if (lang.ContainsKey(kvp.Key) && !lang.ContainsKey(kvp.Value))
					{
						lang.Add(kvp.Value, lang[kvp.Key]);
						lang.Remove(kvp.Key);
						counter++;
					}
					else if (!lang.ContainsKey(kvp.Key))
					{
						lang.Add(kvp.Value, "");
					}
					else
					{
						//multiple matches found
						//in this case we apply the special rule for lucca which consist in concetenating the strings
						lang[kvp.Value] += " <br /><br />" + lang[kvp.Key];
						lang.Remove(kvp.Key);
					}
				}				
			}
		}

	}
}
