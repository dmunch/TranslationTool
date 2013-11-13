using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslationTool.IO.Worksheets
{
	public class MemoryWorksheet : IWorksheet
	{
		public List<List<object>> Cells;

		public int Rows
		{
			get
			{
				return Cells.Count;
			}
		}

		public int Columns
		{
			get
			{
				if (Cells.Count == 0) return 0;

				return Cells[0].Count;
			}
		}

		public MemoryWorksheet()
		{			
			this.Cells = new List<List<object>>();
		}

		public object this[int row, int column]
		{
			get
			{
				row -= 1; column -= 1;

				if (row < 0 || Cells.Count < row)
					return null;

				if (column < 0 || Cells[row].Count < column)
					return null;

				return Cells[row][column];
			}
			set
			{
				row -= 1; column -= 1;

				if (Cells.Count <= row)
					while (Cells.Count <= row)
						Cells.Add(new List<object>());

				if (Cells[row].Count <= column)
					while (Cells[row].Count <= column)
						Cells[row].Add(new List<object>());

				Cells[row][column] = value;
			}
		}
	}
}
