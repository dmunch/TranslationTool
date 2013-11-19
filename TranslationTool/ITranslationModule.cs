using System;
using System.Collections.Generic;

namespace TranslationTool
{
	interface ITranslationModule
	{
		IEnumerable<string> Languages { get; }
		string MasterLanguage { get; }
		string Name { get; set; }

		IEnumerable<string> Keys { get; }
		IEnumerable<SegmentsByKey> ByKey { get; }
		IEnumerable<SegmentsByLanguage> ByLanguage(string lang);

		TranslationModuleDiff Diff(TranslationModule tp);				
		void Patch(TranslationModuleDiff tpDiff);		
		TranslationModuleDiff SyncWith(ITranslationModule tp);
	}
}
