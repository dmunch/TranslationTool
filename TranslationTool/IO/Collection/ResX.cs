using System.Collections.Generic;

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
		
		public void ToResX(TranslationProjectCollection tpc, string targetDir)
		{
			foreach (var tp in tpc.Projects)
				IO.ResX.ToResX(tp.Value, targetDir);
		}
	}
}
