using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslationTool
{
	public interface ITranslationProject
	{
		IEnumerable<string> ModuleNames { get; }
		IEnumerable<ITranslationModule> Modules { get; }

		ITranslationModule this[string moduleName] { get; }
		void Add(ITranslationModule module);

		void SyncWith(ITranslationProject other);
	}
}
