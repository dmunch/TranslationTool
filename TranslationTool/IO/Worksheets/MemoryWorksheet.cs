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
				Resize(row, column);
				Cells[row][column] = value;
			}
		}

		protected void Resize(int rows, int columns)
		{
			if (Cells.Count <= rows)
				while (Cells.Count <= rows)
					Cells.Add(new List<object>());

			if (Cells[rows].Count <= columns)
				while (Cells[rows].Count <= columns)
					Cells[rows].Add(new List<object>());

		}
	}
}
