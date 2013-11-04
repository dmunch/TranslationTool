using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslationTool.IO
{
	public interface IWorksheet
	{
		object this[int row, int column]
		{
			get;
			set;
		}
	}
	public static class Export
	{
		public static int ToIWorksheet(TranslationProject project, IWorksheet worksheet, int rowStart = 1)
			{
				int columnCounter = 1;
				int rowCounter = rowStart;

				if (rowStart == 1) //write header
				{
					worksheet[rowStart, columnCounter++] = "";

					foreach (var l in project.Languages)
						worksheet[rowStart, columnCounter++] = l;
					rowCounter++;
				}

				foreach (var key in project.Keys)
				{
					columnCounter = 1;
					worksheet[rowCounter, columnCounter++] = key;

					foreach (var l in project.Languages)
						worksheet[rowCounter, columnCounter++] = project.Dicts.ContainsKey(l) ? project.Dicts[l].ContainsKey(key) ? project.Dicts[l][key] : "" : "";
					rowCounter++;
				}

				return rowCounter;
			}
	}
}
