﻿using System;
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
		public KeyMatcher(TranslationModule tpBase, TranslationModule tp, string masterLanguage = null)
		{
			var memory = new TranslationMemory(tpBase);
			Matches = new Dictionary<string,string>();
			NoMatches = new List<string>();

			masterLanguage = masterLanguage ?? tp.MasterLanguage;

			foreach (var seg in tp.ByLanguage[masterLanguage])
			{
				var results = memory.Query(masterLanguage, seg.Text);

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
		public void ChangeKeys(TranslationModule tp, bool concatMultipleMatches = false)
		{
			int counter = 0;
			var byLanguage = tp.ByLanguage;

			foreach(var kvp in Matches.Where(kvp => kvp.Key != kvp.Value))
			{				
				foreach(var lang in byLanguage)
				{
					var newKey = lang.Where(s => s.Key == kvp.Key);
					var existingKey = lang.Where(s => s.Key == kvp.Value);

					if (newKey.Any() && !existingKey.Any())
					{
						//rename keys
						foreach (var s in newKey)
						{
							s.Key = kvp.Key;
						}
					}
					else if (!existingKey.Any())
					{
						//add empty value
						tp.Add(new Segment(lang.Key, kvp.Value, ""));
					}
					else if (concatMultipleMatches)
					{
						//multiple matches found
						//in this case we apply the special rule for lucca which consist in concetenating the strings

						throw new NotImplementedException();
					}

					/*
					//var lang = tp.Dicts[langKey];
					if(lang)
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
					else if(concatMultipleMatches)
					{
						//multiple matches found
						//in this case we apply the special rule for lucca which consist in concetenating the strings
						lang[kvp.Value] += " <br /><br />" + lang[kvp.Key];
						lang.Remove(kvp.Key);
					}*/
				}				
			}
		}

	}
}
