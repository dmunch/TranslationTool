using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TranslationTool.IO;

namespace TranslationTool
{
    public class TranslationProjectCollection
    {
		public Dictionary<string, TranslationProject> Projects { get; set; }
		public IEnumerable<string> ProjectNames
		{
			get
			{
				return Projects.Keys;
			}
		}

		public TranslationProjectCollection(Dictionary<string, TranslationProject> dict)
		{
			this.Projects = dict;
		}

        public TranslationProjectCollection()
        {
            this.Projects = new Dictionary<string, TranslationProject>();
        }
                       
        public void SyncWith(TranslationProjectCollection tpc)
        {
            foreach (var tp in Projects)
                if(tpc.Projects.ContainsKey(tp.Key))
                    tp.Value.SyncWith(tpc.Projects[tp.Key]);
        }
    }
}
