using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslationTool.IO.Collection
{
	class Export
	{
		public void ToIWorksheet(TranslationProjectCollection tpc, IWorksheet worksheet)
		{
			//write header
			int columnCount = 1;
			worksheet[1, columnCount++] = "";
			worksheet[1, columnCount++] = tpc.Projects.First().Value.MasterLanguage;

			foreach (var l in tpc.Projects.First().Value.Languages)
				worksheet[1, columnCount++] = l;

			int rowCount = 2;
			foreach (var kvp in tpc.Projects)
			{
				worksheet[rowCount, 1] = "ns:" + kvp.Key;
				rowCount++;

				rowCount = IO.Export.ToIWorksheet(kvp.Value, worksheet, rowCount);
			}
		}
	}
}
