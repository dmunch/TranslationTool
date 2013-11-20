using System;
using System.Collections.Generic;
using System.Linq;

namespace TranslationTool
{
	public interface ITranslationModule
	{
		void Add(IEnumerable<Segment> s);
		void Add(Segment s);
		ILookup<string, Segment> ByKey { get; }
		ILookup<string, Segment> ByLanguage { get; }
		bool ContainsKey(string key);
		IEnumerable<string> Keys { get; }
		void Patch(TranslationModuleDiff tpDiff);
		void Remove(IEnumerable<Segment> segments);
		void RemoveKeys(IEnumerable<string> keys);
		IEnumerable<Segment> Segments { get; }
	}
}
