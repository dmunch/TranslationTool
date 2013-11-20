using TranslationTool.Helpers;

namespace TranslationTool
{
	public class TranslationProjectDiff : GeneralDiff<TranslationModule, string, System.DateTime>
	{
		public TranslationProjectDiff()
			: base(p => p.Name, p => p.LastModified)
		{
			this.IsModifiedComparer = (d1, d2) => d1 < d2;
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
