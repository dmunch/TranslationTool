using TranslationTool.Helpers;

namespace TranslationTool
{
	public class SegmentDiff : StringContentDiff<Segment, string>
	{
		public SegmentDiff()
			: base(s => s.Key, s => s.Text)
		{

		}

		public SegmentDiff(GeneralDiff<Segment, string, string> gd)
			: this()
		{
			this.New = gd.New;
			this.Orig = gd.Orig;
			this.Updated = gd.Updated;
		}
	}
}
