using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TranslationTool.IO;

namespace TranslationTool
{
	public interface ITranslationProject
	{
		IEnumerable<string> Modules { get; }
		TranslationModule this[string moduleName] { get; set; }
	}

    public class TranslationModuleCollection
    {
		public Dictionary<string, TranslationModule> Projects { get; set; }
		public IEnumerable<string> ProjectNames
		{
			get
			{
				return Projects.Keys;
			}
		}

		public TranslationModuleCollection(Dictionary<string, TranslationModule> dict)
		{
			this.Projects = dict;
		}

        public TranslationModuleCollection()
        {
            this.Projects = new Dictionary<string, TranslationModule>();
        }
                       
        public void SyncWith(TranslationModuleCollection tpc)
        {
            foreach (var tp in Projects)
                if(tpc.Projects.ContainsKey(tp.Key))
                    tp.Value.SyncWith(tpc.Projects[tp.Key]);
        }
    }
}
