﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslationTool.Helpers
{
	class MessageFormatter
	{
		private class FormatItem
		{
			public int index; //index of item in the argument list. -1 means it's a literal from the original format string
			public char[] value; //literal data from original format string
			public string format; //simple format to use with supplied argument (ie: {0:X} for Hex

			// for fixed-width format (examples below) 
			public int width;    // {0,7} means it should be at least 7 characters   
			public bool justify; // {0,-7} would use opposite alignment
		}

		//this data is all populated by the constructor
		private List<FormatItem> parts = new List<FormatItem>();
		private int baseSize = 0;
		private string format;
		private IFormatProvider formatProvider = null;
		private ICustomFormatter customFormatter = null;

		// the code in here very closely matches the code in the String.Format/StringBuilder.AppendFormat methods.  
		// Could it be faster?
		public String Format(params Object[] args)
		{
			if (format == null || args == null)
				throw new ArgumentNullException((format == null) ? "format" : "args");

			var sb = new StringBuilder(baseSize);
			foreach (FormatItem fi in parts)
			{
				if (fi.index < 0)
					sb.Append(fi.value);
				else
				{
					//if (fi.index >= args.Length) throw new FormatException(Environment.GetResourceString("Format_IndexOutOfRange"));
					if (fi.index >= args.Length) throw new FormatException("Format_IndexOutOfRange");

					object arg = args[fi.index];
					string s = null;
					if (customFormatter != null)
					{
						s = customFormatter.Format(fi.format, arg, formatProvider);
					}

					if (s == null)
					{
						if (arg is IFormattable)
						{
							s = ((IFormattable)arg).ToString(fi.format, formatProvider);
						}
						else if (arg != null)
						{
							s = arg.ToString();
						}
					}

					if (s == null) s = String.Empty;
					int pad = fi.width - s.Length;
					if (!fi.justify && pad > 0) sb.Append(' ', pad);
					sb.Append(s);
					if (fi.justify && pad > 0) sb.Append(' ', pad);
				}
			}
			return sb.ToString();
		}

		//alternate implementation (for comparative testing)
		// my own test call String.Format() separately: I don't use this.  But it's useful to see
		// how my format method fits.
		public string OriginalFormat(params Object[] args)
		{
			return String.Format(formatProvider, format, args);
		}
	}
}
