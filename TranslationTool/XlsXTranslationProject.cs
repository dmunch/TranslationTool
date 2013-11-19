using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslationTool
{
	class XlsXTranslationProject : ITranslationProject
	{

		public TranslationModule this[string moduleName]
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public IEnumerable<string> Modules
		{
			get { throw new NotImplementedException(); }
		}
	}

	class ResXTranslationProject : ITranslationProject
	{
		TranslationModuleCollection Collection;
		string Directory;
		string MasterLanguage;

		public ResXTranslationProject(string directory, string masterLanguage)
		{
			this.Collection = new TranslationModuleCollection();

			this.Modules = IO.Collection.ResX.GetModuleNames(directory);
			this.MasterLanguage = masterLanguage;
		}

		public TranslationModule this[string moduleName]
		{
			get
			{
				if (!Collection.Projects.ContainsKey(moduleName) && Modules.Contains(moduleName))
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

		public IEnumerable<string> Modules { get; set;}
	}
}
