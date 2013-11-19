using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslationTool
{
	class ResXTranslationProject : ITranslationProject
	{
		TranslationModuleCollection Collection;
		string Directory;
		string MasterLanguage;

		public ResXTranslationProject(string directory, string masterLanguage)
		{
			this.Collection = new TranslationModuleCollection();

			this.ModuleNames= IO.Collection.ResX.GetModuleNames(directory);
			this.MasterLanguage = masterLanguage;
		}

		public TranslationModule this[string moduleName]
		{
			get
			{
				if (!Collection.Projects.ContainsKey(moduleName) && ModuleNames.Contains(moduleName))
				{
					Collection.Projects.Add(moduleName, IO.ResX.FromResX(this.Directory, moduleName, this.MasterLanguage));
				}
				
					
				return Collection.Projects[moduleName];
			}

			set
			{
				if(!Collection.Projects.ContainsKey(moduleName))
				{
					Collection.Projects.Add(moduleName, value);
					IO.ResX.ToResX(value, this.Directory);
				}
			}
		}

		public IEnumerable<string> ModuleNames { get; set;}
		public IEnumerable<TranslationModule> Modules
		{
			get
			{
				return ModuleNames.Select(name => this[name]);
			}
		}

		public void Add(TranslationModule module)
		{
			throw new NotImplementedException();
		}

		public void SyncWith(ITranslationProject other)
		{
			throw new NotImplementedException();
		}
	}
}
