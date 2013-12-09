using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslationTool
{
	public class TranslationModuleBase
	{
		public IEnumerable<string> Languages { get; protected set; }
		public string MasterLanguage { get; protected set; }
		public string Name { get; set; }
		public DateTime LastModified { get; set; }

		public TranslationModuleBase(TranslationModuleBase other)
		{
			this.MasterLanguage = other.MasterLanguage;
			this.Languages = other.Languages;
			this.Name = other.Name;
			this.LastModified = other.LastModified;
		}

		public TranslationModuleBase(string name, string masterLanguage)
			: this(name, masterLanguage, new string[] { "en", "de", "es", "fr", "it", "nl" })
		{
		}

		public TranslationModuleBase(string name, string masterLanguage, string[] languages)
		{
			this.MasterLanguage = masterLanguage;
			this.Languages = languages;

			this.Name = name;
			this.LastModified = DateTime.Now;
		}

		public static void PrintSynced(Dictionary<string, string> synced, string language)
		{
			Console.WriteLine("Synced {0} rows in {1}.", synced.Count, language);
			foreach (var kvp in synced)
				Console.WriteLine(kvp.Key);

		}
	}

}
