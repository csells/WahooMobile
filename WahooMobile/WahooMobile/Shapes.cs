using System;
using System.Collections;
using System.Diagnostics;
using Xamarin.Forms;

namespace WahooMobile {
  abstract class Shape {
    protected Game game;
    protected Color color;
    protected int row = 0;
    protected int col = 0;
    protected int position = 0;
    protected Position[] positions;

    protected Shape(Game game, Color color, Position[] positions) {
      this.game = game;
      this.color = color;
      this.positions = positions;
    }

    public void Draw(AbsoluteLayout layout) {
      this.positions[this.position].Draw(this.game, layout, this.color, this.row, this.col);
    }

    public int Row {
      get { return this.row; }
    }

    public int Column {
      get { return this.col; }
    }

    public int Width {
      get { return this.positions[this.position].Width; }
    }

    public int Height {
      get { return this.positions[this.position].Height; }
    }

    public Color Color {
      get { return this.color; }
    }

    public bool CellFilled(int row, int col) {
      return this.positions[this.position].CellFilled(row, col);
    }

    public bool Down() {
      // Check if one more move will put us below the bottom
      if (this.row + Height + 1 > this.game.Rows) { return false; }

      // Check for collisions
      if (Collides(this.row + 1, this.col)) { return false; }

      // Move down a row
      ++this.row;
      return true;
    }

    public bool Left() {
      // Check if one more move right will put us beyond the edge
      if (this.col - 1 < 0) { return false; }

      // Check for collisions
      if (Collides(this.row, this.col - 1)) { return false; }

      // Move over one column
      --this.col;
      return true;
    }

    public bool Right() {
      // Check if one more move right will put us beyond the edge
      if (this.col + Width + 1 > this.game.Columns) { return false; }

      // Check for collisions
      if (Collides(this.row, this.col + 1)) { return false; }

      // Move over one column
      ++this.col;
      return true;
    }

    private bool Rotate(int dir) {
      int newPosition = this.position + dir;
      if (newPosition >= this.positions.Length) { newPosition -= this.positions.Length; }
      else if (newPosition < 0) { newPosition += this.positions.Length; }

      // Can't rotate if it would collide
      if (Collides(this.row, this.col, newPosition)) { return false; }

      this.position = newPosition;
      return true;
    }

    public bool RotateLeft() {
      return Rotate(-1);
    }

    public bool RotateRight() {
      return Rotate(1);
    }

    protected bool Collides(int row, int col) {
      return Collides(row, col, this.position);
    }

    protected bool Collides(int row, int col, int position) {
      return this.positions[position].Collides(this.game, row, col);
    }

    protected class Position {
      BitArray[] masks;

      public Position(params string[] masks) {
        this.masks = new BitArray[masks.Length];
        for (int i = 0; i != masks.Length; ++i) {
          this.masks[i] = new BitArray(masks[i].Length);
          for (int j = 0; j != masks[i].Length; ++j) {
            switch (masks[i][j]) {
              case '0': /* bit already set to false */ break;
              case '1': this.masks[i][j] = true; break;
              default: Debug.Assert(false, "Only 0s and 1s accepted in bitmasks"); break;
            }
          }
        }
      }

      public void Draw(Game game, AbsoluteLayout layout, Color color, int rowOffset, int colOffset) {
        for (int row = 0; row != this.masks.Length; ++row) {
          for (int col = 0; col != this.masks[row].Length; ++col) {
            if (this.masks[row][col]) {
              game.DrawCell(layout, color, row + rowOffset, col + colOffset);
            }
          }
        }
      }

      public bool Collides(Game game, int rowOffset, int colOffset) {
        for (int row = 0; row != this.masks.Length; ++row) {
          for (int col = 0; col != this.masks[row].Length; ++col) {
            // If mask is set at this position and cell is filled, that's a collision
            if (this.masks[row][col] && game.CellFilled(row + rowOffset, col + colOffset)) return true;
          }
        }

        return false;
      }

      public bool CellFilled(int row, int col) {
        return this.masks[row][col];
      }

      public int Width {
        get { return this.masks[0].Length; }
      }

      public int Height {
        get { return this.masks.Length; }
      }
    }
  }

  class ShapeT : Shape {
    public ShapeT(Game game) :
      base(game,
             Color.Red,
             new Position[] {
                                    new Position("010",
                                                 "111"),

                                    new Position("10",
                                                 "11",
                                                 "10"),

                                    new Position("111",
                                                 "010"),

                                    new Position("01",
                                                 "11",
                                                 "01"),
                                }) {
    }
  }

  class ShapeCrook1 : Shape {
    public ShapeCrook1(Game game) :
      base(game,
             Color.Green,
             new Position[] {
                                    new Position("110",
                                                 "011"),

                                    new Position("01",
                                                 "11",
                                                 "10")
                                    }) {
    }
  }

  class ShapeCrook2 : Shape {
    public ShapeCrook2(Game game) :
      base(game,
             Color.Navy,
             new Position[] {
                                    new Position("011",
                                                 "110"),

                                    new Position("10",
                                                 "11",
                                                 "01")
                                    }) {
    }
  }

  class ShapeL1 : Shape {
    public ShapeL1(Game game) :
      base(game,
             Color.Blue,
             new Position[] {
                                    new Position("001",
                                                 "111"),

                                    new Position("10",
                                                 "10",
                                                 "11"),

                                    new Position("111",
                                                 "100"),

                                    new Position("11",
                                                 "01",
                                                 "01")
                                    }) {
    }
  }

  class ShapeL2 : Shape {
    public ShapeL2(Game game) :
      base(game,
             Color.Aqua,
             new Position[] {
                                    new Position("100",
                                                 "111"),

                                    new Position("11",
                                                 "10",
                                                 "10"),

                                    new Position("111",
                                                 "001"),

                                    new Position("01",
                                                 "01",
                                                 "11")
                                    }) {
    }
  }

  class ShapeLine : Shape {
    public ShapeLine(Game game) :
      base(game,
             Color.Purple,
             new Position[] {
                                    new Position("1111"),

                                    new Position("1",
                                                 "1",
                                                 "1",
                                                 "1")
                                    }) {
    }
  }

  class ShapeBox : Shape {
    public ShapeBox(Game game) :
      base(game,
             Color.Lime,
             new Position[] {
                                    new Position("11",
                                                 "11")
                                    }) {
    }
  }
}
