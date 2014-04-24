using TranslationTool.Helpers;
using System.Collections.Generic;

namespace TranslationTool
{
	public class TranslationProjectDiff : GeneralDiff<ITranslationModule, string, System.DateTime>
	{
		public IDictionary<string, TranslationModuleDiff> DiffPerModule { get; protected set; }

		public TranslationProjectDiff()
			: base(p => p.Name, p => p.LastModified)
		{
			//this.IsModifiedComparer = (d1, d2) => d1 < d2;
			this.IsModifiedComparer = (d1, d2) => true;
		}


		public void Diff(ITranslationProject project1, ITranslationProject project2)
		{
			base.Diff(project1.Modules, project2.Modules);

			DiffPerModule = new Dictionary<string, TranslationModuleDiff>();

			foreach (var tm in this.Updated)
			{
				var tmOrig = this.Orig[tm.Key];
				var tmNew = tm.Value;

				DiffPerModule.Add(tmOrig.Name, tmOrig.Diff(tmNew));
			}
				/*if (list2.ModuleNames.Contains(tm.Name))
					yield return tm.Diff(other[tm.Name]);*/
		}

		public void Print(ILogging logger)
		{
			foreach (var newModule in this.New)
			{
				logger.WriteLine("<h2>New: {0}</h2>", newModule.Value.Name);
			}

			foreach (var moduleDiff in this.DiffPerModule)
			{
				logger.WriteLine("<h2>{0}</h2>", moduleDiff.Key);
				moduleDiff.Value.Print(logger);
			}
		}
	}

	public static class ProjectExtensionMethods
	{
		public static TranslationProjectDiff Diff(this ITranslationProject p, ITranslationProject other)
		{
			var tpd = new TranslationProjectDiff();

			tpd.Diff(p.Modules, other.Modules);			
			return tpd;
		}
	}
}
