using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridSetter.ViewModels
{
    public class GridViewModel
    {
        public int NbCol { get; set; }
        public int NbRow { get; set; }
        public List<CellViewModel> Cells { get; set; }

        public GridViewModel(int nbCol, int nbRow)
        {
            NbCol = nbCol;
            NbRow = nbRow;
            Cells = new List<CellViewModel>();
        }
    }

    public class CellViewModel
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public int Col { get; set; }
        public int Row { get; set; }
        public int ColSpan { get; set; }
        public int RowSpan { get; set; }
        public string Source { get; set; }
        public List<InnerCellViewModel> InnerCells { get; set; }

        public CellViewModel(double width, double height, int col, int row, int colSpan, int rowSpan, string source, List<InnerCellViewModel> innerCells = null)
        {
            Width = width;
            Height = height;
            Col = col;
            Row = row;
            ColSpan = colSpan;
            RowSpan = rowSpan;
            Source = source;
            InnerCells = innerCells;
        }
    }

    public class InnerCellViewModel
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public int Col { get; set; }
        public int Row { get; set; }

        public InnerCellViewModel(double width, double height, int col, int row)
        {
            Width = width;
            Height = height;
            Col = col;
            Row = row;
        }
    }
}
