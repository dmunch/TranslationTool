using System;
using System.Collections.Generic;
using System.Linq;

namespace TranslationTool
{
	public interface ITranslationModuleBase
	{
		string MasterLanguage { get; }
		IEnumerable<string> Languages { get; }
		string Name { get; }
		DateTime LastModified { get; }
	}

	public interface ITranslationModule :  ITranslationModuleBase
	{		
		void Add(IEnumerable<Segment> s);
		void Add(Segment s);
		ILookup<string, Segment> ByKey { get; }
		ILookup<string, Segment> ByLanguage { get; }
		bool ContainsKey(string key);
		IEnumerable<string> Keys { get; }

		TranslationModuleDiff SyncWith(ITranslationModule module);
		TranslationModuleDiff Diff(ITranslationModule other);

		void Patch(TranslationModuleDiff tpDiff);
		void Remove(IEnumerable<Segment> segments);
		void RemoveKeys(IEnumerable<string> keys);
		IEnumerable<Segment> Segments { get; }
	}
}
