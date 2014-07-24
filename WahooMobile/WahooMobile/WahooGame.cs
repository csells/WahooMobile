using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Labs;
using Xamarin.Forms.Labs.Services;

namespace WahooMobile {
  class WahooGame : ContentPage {
    public delegate void GameOverCallback();
    public delegate void LinesRemovedCallback(int linesRemoved);

    public event GameOverCallback GameOver;
    public event LinesRemovedCallback LinesRemoved;

    int linesRemoved;
    Game game = new Game { Rows = 20, Columns = 10, Padding = 2 };

    protected override void OnAppearing() {
      base.OnAppearing();
      TickInterval = 500;
      var display = Resolver.Resolve<IDisplay>();
      this.game.Size = new Size { Width = display.Width, Height = display.Height };

      New(); // TODO: remove
    }

    public int TickInterval { get; set; }
    public int Speed { get; private set; }
    public int Rows { get; set; }
    public int Columns { get; set; }
    public int Padding { get; set; }
    public bool IsPaused { get; set; }
    public bool IsPlaying { get; set; }

    public void New() {
      this.game.Reset();
      this.linesRemoved = 0;
      IsPlaying = true;
      IsPaused = false;
      StartTimer();
    }

    public void Pause() {
      IsPaused = true;
    }

    public void Resume() {
      IsPaused = false;
      StartTimer();
    }

    void StartTimer() {
      Device.StartTimer(TimeSpan.FromMilliseconds(TickInterval), timer_Tick);
    }

    /*
    public bool ProcessKey(Keys keyData) {
      if (DesignMode || !_isPlaying || _isPaused) return false;

      // Check for the arrow keys, too
      bool processed = false;
      if (keyData == _dropKey || keyData == Keys.Down) processed = this.game.Drop();
      else if (keyData == _leftKey || keyData == Keys.Left) processed = this.game.Left();
      else if (keyData == _rightKey || keyData == Keys.Right) processed = this.game.Right();
      else if (keyData == _rotateRightKey || keyData == Keys.Up) processed = this.game.RotateRight();
      else if (keyData == _rotateLeftKey) processed = this.game.RotateLeft();

      if (processed) Invalidate();
      return processed;
    }
    */

    void Draw() {
      var layout = new AbsoluteLayout() { BackgroundColor = Color.White };
      this.game.Draw(layout);
      //DumpLayout(layout);
      Content = layout;
    }

    #region DumpLayout
#if false
    void DumpLayout(AbsoluteLayout layout) {
      Debug.WriteLine(string.Format("layout: {0} cells", layout.Children.Count));
      foreach (var child in layout.Children) {
        var bounds = AbsoluteLayout.GetLayoutBounds(child);
        Debug.WriteLine(string.Format(
          "\tcolor={0}, x= {1}, y= {2}, width= {3}, height= {4}",
          ColorName(((BoxView)child).Color),
          bounds.Left,
          bounds.Top,
          bounds.Width,
          bounds.Height
        ));
      }
    }

    string ColorName(Color color) {
      if (color == Color.Red) { return "red"; }
      else if (color == Color.Green) { return "green"; }
      else if (color == Color.Navy) { return "navy"; }
      else if (color == Color.Blue) { return "blue"; }
      else if (color == Color.Aqua) { return "aqua"; }
      else if (color == Color.Purple) { return "purple"; }
      else if (color == Color.Lime) { return "lime"; }
      else { Debug.Assert(false, "unknown color"); return "<unknown>"; }
    }
#endif
    #endregion

    // TODO: the size doesn't change, but the orientation can
    //protected override void OnSizeChanged(EventArgs e) {
    //  this.game.Size = ClientSize;
    //  Invalidate();
    //}

    bool timer_Tick() {
      // Stop the timer when the game stops or pauses
      if (!IsPlaying || IsPaused) { return false; }

      // Update the game
      this.game.Tick();

      // Draw the game
      Draw();

      // Check for removed lines
      if (this.game.LinesRemoved > this.linesRemoved) {
        if (LinesRemoved != null) { LinesRemoved(this.game.LinesRemoved - this.linesRemoved); }
        this.linesRemoved = this.game.LinesRemoved;

        // Increase the speed (in chunks)
        if (TickInterval - this.linesRemoved * 10 > 0) {
          Speed = (this.linesRemoved / 10) * 100;
          Device.StartTimer(TimeSpan.FromMilliseconds(TickInterval - Speed), timer_Tick);
        }
      }

      // Check for game over
      if (this.game.IsOver) {
        IsPlaying = false;
        IsPaused = false;
        if (GameOver != null) { GameOver(); }
      }

      // Stop the timer if the game is over
      return IsPlaying && !IsPaused;
    }

  }

}
