using System;
using System.Collections;
using System.Diagnostics;
using Xamarin.Forms;

namespace WahooMobile {
  struct Size {
    public int Width;
    public int Height;
  };

  class Game {
    Size size;
    int rows;
    int cols;
    int padding;
    bool isOver;
    int linesRemoved;
    Shape currentShape;
    Random rnd = new Random();
    Color[,] cellColors;

    public void Tick() {
      // When the current shape gets to the bottom...
      if (!this.currentShape.Down()) {
        // If we collide on the first line, game over
        if (this.currentShape.Row == 0) {
          this.isOver = true;
          return;
        }

        // Move current shape to set of filled cells
        AcquireCurrentShape();

        // Remove the filled lines (and update the score)
        int fillsFound = CheckFills(this.currentShape.Row, this.currentShape.Height);
        this.linesRemoved += fillsFound;

        // Create a new shape
        this.currentShape = GetRandomShape();
      }
    }

    public void Draw(AbsoluteLayout layout) {
      // Draw the board background
      Rectangle topLeftCell = GetCellRectangle(this.size, this.rows, this.cols, this.padding, 0, 0);
      Rectangle bottomRightCell = GetCellRectangle(this.size, this.rows, this.cols, this.padding, this.rows - 1, this.cols - 1);
      Rectangle gameRect = new Rectangle(topLeftCell.Left, topLeftCell.Top, bottomRightCell.Right - topLeftCell.Left, bottomRightCell.Bottom - topLeftCell.Top);
      //DrawRect(layout, gameRect, Color.White);

      // Draw the current shape
      if (this.currentShape != null) { this.currentShape.Draw(layout); }

      // Draw the colored cells left behind by old shapes
      for (int row = 0; row != this.rows; ++row) {
        for (int col = 0; col != this.cols; ++col) {
          if (CellFilled(row, col)) { DrawCell(layout, this.cellColors[row, col], row, col); }
        }
      }
    }

    void DrawRect(AbsoluteLayout layout, Rectangle bounds, Color fill) {
      var box = new BoxView() { Color = fill };
      AbsoluteLayout.SetLayoutBounds(box, bounds);
      layout.Children.Add(box);
    }

    public bool CellFilled(int row, int col) {
      // If we're out of bounds, report that as filled
      // That will cause the edges to act as a boundary
      // and prevent turning a shape out of bounds
      if (row < 0 || row >= this.rows) { return true; }
      if (col < 0 || col >= this.cols) { return true; }

      // Check the visible cells
      return this.cellColors[row, col] != Color.Transparent;
    }

    public bool Drop() {
      // Drop as far as we can
      while (this.currentShape.Down()) ;
      return true;
    }

    public bool Left() {
      return this.currentShape.Left();
    }

    public bool Right() {
      return this.currentShape.Right();
    }

    public bool RotateRight() {
      return this.currentShape.RotateRight();
    }

    public bool RotateLeft() {
      return this.currentShape.RotateLeft();
    }

    public void Reset() {
      ResetRowCol();
      this.currentShape = GetRandomShape();
    }

    protected Shape GetRandomShape() {
      Shape shape = null;

      switch (this.rnd.Next(0, 7)) {
        case 0: shape = new ShapeT(this); break;
        case 1: shape = new ShapeCrook1(this); break;
        case 2: shape = new ShapeCrook2(this); break;
        case 3: shape = new ShapeL1(this); break;
        case 4: shape = new ShapeL2(this); break;
        case 5: shape = new ShapeLine(this); break;
        case 6: shape = new ShapeBox(this); break;
        default: Debug.Assert(false, "Check your random usage"); return null;
      }

      int colCentered = (this.cols - shape.Width) / 2;
      while (colCentered-- != 0) { shape.Right(); }
      return shape;
    }

    protected void FillCell(int row, int col, Color color) {
      this.cellColors[row, col] = color;
    }

    protected void ClearCell(int row, int col) {
      this.cellColors[row, col] = Color.Transparent;
    }

    protected Color CellColor(int row, int col) {
      return this.cellColors[row, col];
    }

    protected void AcquireCurrentShape() {
      // Make all currently set cells in the shape part of the cells of the game
      Color color = this.currentShape.Color;
      for (int row = 0; row != this.currentShape.Height; ++row) {
        for (int col = 0; col != this.currentShape.Width; ++col) {
          if (this.currentShape.CellFilled(row, col)) {
            FillCell(row + this.currentShape.Row, col + this.currentShape.Column, color);
          }
        }
      }
    }

    protected void MoveLines(int bottomRow) {
      // Move bottomRow to not quite the top
      for (int row = bottomRow - 1; row > 0; --row) {
        for (int col = 0; col != this.cols; ++col) {
          FillCell(row + 1, col, CellColor(row, col));
        }
      }

      // Clear the top row
      for (int col = 0; col != this.cols; ++col) {
        ClearCell(0, col);
      }
    }

    protected int CheckFills(int topRow, int height) {
      int filledLines = 0;

      for (int row = topRow; row != topRow + height; ++row) {
        bool lineFilled = true;
        for (int col = 0; col != this.cols; ++col) {
          if (!CellFilled(row, col)) { lineFilled = false; break; }
        }

        if (lineFilled) {
          ++filledLines;
          MoveLines(row);
          --row; // Check this line again
        }
      }

      return filledLines;
    }

    public bool IsOver {
      get { return this.isOver; }
    }

    public int LinesRemoved {
      get { return this.linesRemoved; }
    }

    public Size Size {
      get { return this.size; }
      set { this.size = value; }
    }

    public int Rows {
      get { return this.rows; }
      set { this.rows = value; ResetRowCol(); }
    }

    public int Columns {
      get { return this.cols; }
      set { this.cols = value; ResetRowCol(); }
    }

    public int Padding {
      get { return this.padding; }
      set { this.padding = value; }
    }

    protected void ResetRowCol() {
      this.cellColors = new Color[this.rows, this.cols];
      for (int row = 0; row != this.rows; ++row) {
        for (int col = 0; col != this.cols; ++col) {
          this.cellColors[row, col] = Color.Transparent;
        }
      }
    }

    internal void DrawCell(AbsoluteLayout layout, Color color, int row, int col) {
      DrawRect(layout, GetCellRectangle(this.size, this.rows, this.cols, this.padding, row, col), color);
    }

    // Calculate a cell rectangle given a game size, how many rows and cols in a game,
    // the amount of padding around each cell and the cell in question.
    // NOTE: This is more calculation than we strictly need to do, but it's
    // handy for drawing the design view as well as if these properties change on the fly.
    internal static Rectangle GetCellRectangle(Size size, int rows, int cols, int padding, int row, int col) {
      // This algorithm fills the area defined by size, making each cell a rectangle
      //            int cx = (size.Width - padding * (cols + 1))/cols;
      //            int cy = (size.Height - padding * (rows + 1))/rows;
      //            int x = padding + (cx + padding) * col;
      //            int y = padding + (cy + padding) * row;
      //            return new Rectangle(x, y, cx, cy);

      // This algorithm fills the center of the area defined by size, making each cell a square
      int cx = (size.Width - padding * (cols + 1)) / cols;
      int cy = (size.Height - padding * (rows + 1)) / rows;
      int side = Math.Min(cx, cy);
      int cxTotal = (side + padding) * cols + padding;
      int cyTotal = (side + padding) * rows + padding;
      int leftOffset = (size.Width - cxTotal) / 2;
      int topOffset = (size.Height - cyTotal) / 2;
      int x = leftOffset + padding + (side + padding) * col;
      int y = topOffset + padding + (side + padding) * row;
      return new Rectangle(x, y, side, side);
    }
  }
}
