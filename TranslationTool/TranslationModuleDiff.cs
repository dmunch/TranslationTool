using System.Collections.Generic;
using TranslationTool.Helpers;

namespace TranslationTool
{
	public class TranslationModuleDiff
	{
		public Dictionary<string, SegmentDiff> DiffPerLanguage { get; set; }

		public TranslationModuleDiff(Dictionary<string, SegmentDiff> _diffPerLanguage)
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
