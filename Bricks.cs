using System;
using System.Drawing;
using System.Windows.Forms;

namespace Breakout
{
    internal class Bricks
    {
        //*************************************************************
        //Fields
        //*************************************************************
        private Brick[,] mBricks;

        private int mRows;
        private int mCols;

        //*************************************************************
        //Constructors
        //*************************************************************
        public Bricks()
        {
            mRows = 8;      // Level 1 is 8 rows :contentReference[oaicite:10]{index=10}
            mCols = 14;     // simple fit on your 1482 width form :contentReference[oaicite:11]{index=11}
            mBricks = new Brick[mRows, mCols];
        }

        //*************************************************************
        //Properties
        //*************************************************************
        public int Rows { get { return mRows; } }
        public int Cols { get { return mCols; } }

        //*************************************************************
        //Methods
        //*************************************************************
        public void CreateLevel(int level, int screenW, int topY)
        {
            // brick area is top third of screen (assignment description) :contentReference[oaicite:12]{index=12}
            float marginX = 5f;
            float gap = 2f;

            float brickW = (screenW - marginX * 2) / mCols;
            float brickH = 22f;

            // Level 1 rules: two rows each color, bottom->top: yellow, green, orange, red :contentReference[oaicite:13]{index=13}
            for (int r = 0; r < mRows; r++)
            {
                Color c;
                int pts;

                // r=0 is top
                if (r <= 1) { c = Color.Red; pts = 7; }
                else if (r <= 3) { c = Color.Orange; pts = 5; }
                else if (r <= 5) { c = Color.Green; pts = 3; }
                else { c = Color.Yellow; pts = 1; }

                for (int col = 0; col < mCols; col++)
                {
                    float x = marginX + col * brickW;
                    float y = topY + r * brickH;

                    RectangleF rect = new RectangleF(x, y, brickW - gap, brickH - gap);

                    BrickTypes type = BrickTypes.Regular;
                    int hits = 1;
                    Color drawColor = c;
                    int points = pts;

                    // Level 2/3 additions (to reach Level 4 quality): special bricks :contentReference[oaicite:14]{index=14}
                    // Keep it simple: small patterns based on (r+col+level)
                    if (level >= 2)
                    {
                        int k = (r + col + level) % 12;

                        if (k == 0) { type = BrickTypes.Strong; hits = 2; drawColor = ControlPaint.Light(c); points = pts + 2; }
                        else if (k == 1) { type = BrickTypes.SpeedUp; drawColor = Color.DeepSkyBlue; points = pts; }
                        else if (k == 2) { type = BrickTypes.SlowDown; drawColor = Color.MediumPurple; points = pts; }
                        else if (k == 3) { type = BrickTypes.DropLong; drawColor = Color.LimeGreen; points = pts; }
                        else if (k == 4) { type = BrickTypes.DropShort; drawColor = Color.HotPink; points = pts; }
                        else if (k == 5) { type = BrickTypes.DropSpeedUp; drawColor = Color.DeepSkyBlue; points = pts; }
                        else if (k == 6) { type = BrickTypes.DropSlowDown; drawColor = Color.MediumPurple; points = pts; }

                    }

                    if (level >= 3)
                    {
                        // Don't place indestructible bricks in the red rows (rows 0 and 1)
                        if (r > 1)
                        {
                            int k2 = (r * 3 + col) % 17;
                            if (k2 == 0)
                            {
                                type = BrickTypes.Indestructible;
                                drawColor = Color.Gray;
                                points = 0;
                            }
                        }
                    }


                    mBricks[r, col] = new Brick(rect, drawColor, points, type, hits);
                }
            }
        }

        public void Draw(Graphics g, int x, int y)
        {
            for (int r = 0; r < mRows; r++)
                for (int c = 0; c < mCols; c++)
                    mBricks[r, c].Draw(g, x, y);
        }

        public bool AllCleared()
        {
            // cleared = all bricks that are NOT indestructible are gone
            for (int r = 0; r < mRows; r++)
            {
                for (int c = 0; c < mCols; c++)
                {
                    Brick b = mBricks[r, c];
                    if (b.Alive && b.Type != BrickTypes.Indestructible)
                        return false;
                }
            }
            return true;
        }

        // Checks collision; returns points; outputs info for special rules
        public int CheckBallCollision(RectangleF ballRect, out BrickTypes hitType, out Color hitColor, out RectangleF hitRect, out bool destroyed)
        {
            hitType = BrickTypes.None;
            hitColor = Color.Empty;
            hitRect = RectangleF.Empty;
            destroyed = false;

            for (int r = 0; r < mRows; r++)
            {
                for (int c = 0; c < mCols; c++)
                {
                    Brick b = mBricks[r, c];
                    if (!b.Intersects(ballRect)) continue;

                    hitType = b.Type;
                    hitColor = b.Color;
                    hitRect = b.Rect;

                    int pts = b.Hit(out destroyed);
                    return pts;
                }
            }

            return 0;
        }

        // True when ALL red bricks are gone (used for paddle shrink rule) :contentReference[oaicite:15]{index=15}
        public bool RedRowCleared()
        {
            for (int r = 0; r <= 1; r++)
            {
                for (int c = 0; c < mCols; c++)
                {
                    if (mBricks[r, c].Alive) return false;
                }
            }
            return true;
        }
    }
}
