// -----------------------------------------------------------------------
// <copyright file="Logging.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace TranslationTool.Helpers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public interface ILogging
	{
		void WriteLine(string output, params object[] p);
		void Write(string output, params object[] p);
	}

	public class ConsoleLogging : ILogging
	{
		public void WriteLine(string output, params object[] p)
		{
			Console.WriteLine(string.Format(output, p));
		}
		public void Write(string output, params object[] p)
		{
			Console.Write(string.Format(output, p));
		}
	}
	public class HTMLConcatLogger : ILogging
	{
		public string HTML
		{
			get
			{
				return stringBuilder.ToString();
			}
		}
		StringBuilder stringBuilder = new StringBuilder();

		public void WriteLine(string output, params object[] p)
		{
			var str = p.Length == 0 ? output : string.Format(output, p);
			stringBuilder.Append(str).Append("<br>");
		}

		public void Write(string output, params object[] p)
		{
			var str = p.Length == 0 ? output : string.Format(output, p);
			stringBuilder.Append(str);
		}
	}

}
