using System.Collections.Generic;
using System.Linq;

namespace TranslationTool.IO.Collection
{
	public class ResX
	{
		public static TranslationProjectCollection FromResX(IEnumerable<string> projectNames, string directory, string masterLanguage)
		{
			var tpc = new TranslationProjectCollection();

			foreach (var pName in projectNames)
				tpc.Projects.Add(pName, IO.ResX.FromResX(directory, pName, masterLanguage));

			return tpc;
		}

		public static TranslationProjectCollection FromResX(string directory, string masterLanguage)
		{
			
			var tpc = new TranslationProjectCollection();

			var files = System.IO.Directory.EnumerateFiles(directory, "*.resx").Select(file => System.IO.Path.GetFileNameWithoutExtension(file));
			files = files.Select(file => file.Split('.')[0]).GroupBy(f => f).Select(g => g.Key);

			foreach (var pName in files)
				tpc.Projects.Add(pName, IO.ResX.FromResX(directory, pName, masterLanguage));

			return tpc;
		}

		public void ToResX(TranslationProjectCollection tpc, string targetDir)
		{
			foreach (var tp in tpc.Projects)
				IO.ResX.ToResX(tp.Value, targetDir);
		}
	}
}
