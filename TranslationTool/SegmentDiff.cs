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

		public void PrintHtml(ILogging logging)
		{

			if (logging == null)
				logging = new ConsoleLogging();

			logging.WriteLine("Updated {0} rows.", Updated.Count);
			foreach (var kvp in Updated)
			{
				logging.WriteLine("K: {0}\n Old: {1} \n New: {2}", KeySelector(kvp.Value), ContentSelector(Orig[kvp.Key]), ContentSelector(kvp.Value));
			}

			logging.WriteLine("Added {0} rows.", New.Count);
			foreach (var kvp in New)
			{
				logging.WriteLine("{0}", kvp.Key);
			}
		}

		public void PrintDiff(ILogging logger)
		{
			var diff = new DiffMatchPatch.diff_match_patch();
			foreach (var kvp in Updated)
			{
				var c1 = ContentSelector(Orig[kvp.Key]);
				var c2 = ContentSelector(kvp.Value);

				if (string.IsNullOrEmpty(c1) || string.IsNullOrEmpty(c2) || string.Equals(c1, c2))
				{
					//no diffs
				}
				else
				{
					var diffs = diff.diff_main(c1, c2);
					diff.diff_cleanupSemantic(diffs);
					logger.WriteLine(string.Format("K: {0}, {1}", kvp.Key, diff.diff_prettyHtml(diffs)));
					/*
					foreach (var d in diffs)
					{
						logger.WriteLine("{0}: {1}", d.operation, d.text);
					}*/
					logger.WriteLine("<hr/>");
				}
			}
			
			foreach (var kvp in New)
			{
				var n = ContentSelector(New[kvp.Key]);
				logger.WriteLine(string.Format("N: {0}, {1}", kvp.Key, n));
				logger.WriteLine("<hr/>");
			}

			foreach (var kvp in Deleted)
			{
				var n = ContentSelector(Deleted[kvp.Key]);

				logger.WriteLine(string.Format("D: {0}, {1}", kvp.Key, n));
				logger.WriteLine("<hr/>");
			}
		}
	}
}
