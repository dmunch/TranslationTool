using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslationTool
{
	public class Segment
	{
		public string Language { get; protected set; }
		public string Key { get; set; }
		public string Text { get; set; }
		public string Comment { get; set; }
		public IEnumerable<string> Tags { get; set; }

		public Segment()
		{
			Tags = Enumerable.Empty<string>();
		}

		public Segment(string lang, string t)
			: this()
		{
			this.Language = lang;
			this.Text = t;
		}

		public Segment(string lang, string key, string t)
			:this()
		{
			this.Language = lang;
			this.Text = t;
			this.Key = key;
		}

		public Segment(Segment other)
			:this()
		{
			this.Language = other.Language;
			this.Text = other.Text;
			this.Key = other.Key;
			this.Tags = other.Tags;
		}

		public override string ToString()
		{
			return "(" + Language + ") " + Key + ": " + Text;
		}

		public static IEnumerable<Segment> FromDict(string language, IDictionary<string, string> dict)
		{
			foreach (var kvp in dict)
			{
				yield return new Segment(language, kvp.Key, kvp.Value);
			}
		}

		public static IEnumerable<Segment> EmptyFromTemplate(IEnumerable<Segment> template)
		{
			return template.Select(t => new Segment(t.Language, t.Key, ""));
		}
	}



	public class SegmentsByKey
	{
		public string Key { get; protected set; }
		public IEnumerable<Segment> Segments { get; protected set; }

		public SegmentsByKey(string key, IEnumerable<Segment> segments)
		{
			this.Key = key;
			this.Segments = segments;
		}
	}

	public class SegmentsByLanguage
	{
		public string Language { get; protected set; }
		public IEnumerable<Segment> Segments { get; protected set; }

		public SegmentsByLanguage(string language, IEnumerable<Segment> segments)
		{
			this.Language = language;
			this.Segments = segments;
		}
	}

}
