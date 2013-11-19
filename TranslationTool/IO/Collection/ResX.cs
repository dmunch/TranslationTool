using System.Collections.Generic;
using System.Linq;

namespace TranslationTool.IO.Collection
{
	public class ResX
	{
		public static TranslationModuleCollection FromResX(IEnumerable<string> projectNames, string directory, string masterLanguage)
		{
			var tpc = new TranslationModuleCollection();

			foreach (var pName in projectNames)
				tpc.Projects.Add(pName, IO.ResX.FromResX(directory, pName, masterLanguage));

			return tpc;
		}

		public static TranslationModuleCollection FromResX(string directory, string masterLanguage)
		{
			
			var tpc = new TranslationModuleCollection();


			foreach (var pName in GetModuleNames(directory))
				tpc.Projects.Add(pName, IO.ResX.FromResX(directory, pName, masterLanguage));

			return tpc;
		}

		public static IEnumerable<string> GetModuleNames(string directory)
		{
			var files = System.IO.Directory.EnumerateFiles(directory, "*.resx").Select(file => System.IO.Path.GetFileNameWithoutExtension(file));
			var modules = files.Select(file => file.Split('.')[0]).GroupBy(f => f).Select(g => g.Key);

			return modules;
		}

		public void ToResX(TranslationModuleCollection tpc, string targetDir)
		{
			foreach (var tp in tpc.Projects)
				IO.ResX.ToResX(tp.Value, targetDir);
		}
	}
}
