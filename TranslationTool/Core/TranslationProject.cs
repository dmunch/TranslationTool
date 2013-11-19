using System.Collections.Generic;
using System.Linq;

namespace TranslationTool
{	
    public class TranslationProject : ITranslationProject
    {
		public Dictionary<string, TranslationModule> Projects { get; set; }
		public IEnumerable<string> ModuleNames
		{
			get
			{
				return Projects.Keys;
			}
		}

		public IEnumerable<TranslationModule> Modules
		{
			get
			{
				return Projects.Values;
			}
		}

		public TranslationProject(Dictionary<string, TranslationModule> dict)
		{
			this.Projects = dict;
		}

        public TranslationProject()
        {
            this.Projects = new Dictionary<string, TranslationModule>();
        }

		public void SyncWith(ITranslationProject other)
        {
            foreach (var tp in Projects)
                if(other.ModuleNames.Contains(tp.Key))
                    tp.Value.SyncWith(other[tp.Key]);
        }
	

		public TranslationModule this[string moduleName]
		{
			get { return Projects.ContainsKey(moduleName) ? Projects[moduleName] : null; }
		}

		public void Add(TranslationModule module)
		{
			Projects.Add(module.Name, module);
		}
	}
}
