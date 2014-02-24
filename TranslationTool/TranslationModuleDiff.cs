using System.Collections.Generic;
using TranslationTool.Helpers;

namespace TranslationTool
{
	public class TranslationModuleDiff : TranslationModuleBase
	{
		public Dictionary<string, SegmentDiff> DiffPerLanguage { get; protected set; }

		public TranslationModuleDiff(TranslationModuleBase other, Dictionary<string, SegmentDiff> _diffPerLanguage)
			: base(other)
		{
			this.DiffPerLanguage = _diffPerLanguage;
		}

		public void Print(ILogging logger)
		{
			foreach (var diff in DiffPerLanguage)
			{				
				diff.Value.PrintDiff(logger);
			}
		}
	}
}
