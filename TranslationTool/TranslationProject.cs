using System.Collections.Generic;
using System.Linq;

namespace TranslationTool
{	
    public class TranslationProject : ITranslationProject
    {
		public Dictionary<string, ITranslationModule> Projects { get; set; }
		public IEnumerable<string> ModuleNames
		{
			get
			{
				return Projects.Keys;
			}
		}

		public IEnumerable<ITranslationModule> Modules
		{
			get
			{
				return Projects.Values;
			}
		}

		public TranslationProject(Dictionary<string, ITranslationModule> dict)
		{
			this.Projects = dict;
		}

        public TranslationProject()
        {
            this.Projects = new Dictionary<string, ITranslationModule>();
        }

		public TranslationProject(IEnumerable<ITranslationModule> modules)			
		{
			this.Projects = modules.ToDictionary(m => m.Name, m => m);
		}

		public void SyncWith(ITranslationProject other)
        {
            foreach (var tp in Projects)
                if(other.ModuleNames.Contains(tp.Key))
                    tp.Value.SyncWith(other[tp.Key]);
        }


		public static IEnumerable<TranslationModuleDiff> Sync(ITranslationProject one, ITranslationProject other)
		{
			foreach (var tm in one.Modules)
				if (other.ModuleNames.Contains(tm.Name))
					yield return tm.SyncWith(other[tm.Name]);
		}

		public static IEnumerable<TranslationModuleDiff> Diff(ITranslationProject one, ITranslationProject other)
		{
			foreach (var tm in one.Modules)
				if (other.ModuleNames.Contains(tm.Name))
					yield return tm.Diff(other[tm.Name]);
		}

		public ITranslationModule this[string moduleName]
		{
			get { return Projects.ContainsKey(moduleName) ? Projects[moduleName] : null; }
		}

		public void Add(ITranslationModule module)
		{
			Projects.Add(module.Name, module);
		}
	}
}
