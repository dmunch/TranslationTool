using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslationTool
{
	public interface ITranslationProject
	{
		IEnumerable<string> ModuleNames { get; }
		IEnumerable<TranslationModule> Modules { get; }

		TranslationModule this[string moduleName] { get; }
		void Add(TranslationModule module);

		void SyncWith(ITranslationProject other);
	}
}
