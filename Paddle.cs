using System;
using System.Drawing;

//*************************************************************
// Paddle Class
// Represents the player-controlled paddle.
// Handles movement, boundary checking, size changes,
// and ball deflection based on hit location.
//*************************************************************

namespace Breakout
{
    internal class Paddle
    {
        //*************************************************************
        //Fields
        //*************************************************************
        private RectangleF mRect;
        private float mSpeed;

        private float mDefaultWidth;

        //*************************************************************
        //Constructors
        //*************************************************************
        public Paddle()
        {
            mRect = new RectangleF(0, 0, 140, 14);
            mDefaultWidth = mRect.Width;
            mSpeed = 12f;
        }

        public Paddle(float x, float y, float w, float h)
        {
            mRect = new RectangleF(x, y, w, h);
            mDefaultWidth = w;
            mSpeed = 12f;
        }

        //*************************************************************
        //Properties
        //*************************************************************
        public RectangleF Rect { get { return mRect; } }

        //*************************************************************
        //Methods
        //*************************************************************
        public void SetPosition(float x, float y)
        {
            mRect.X = x;
            mRect.Y = y;
        }

        public void MoveLeft() { mRect.X -= mSpeed; }
        public void MoveRight() { mRect.X += mSpeed; }

        public void KeepInBounds(int screenW)
        {
            if (mRect.X < 0) mRect.X = 0;
            if (mRect.Right > screenW) mRect.X = screenW - mRect.Width;
        }

        public void ResetSize()
        {
            mRect.Width = mDefaultWidth;
        }

        public void ShrinkHalf()
        {
            mRect.Width = mDefaultWidth / 2f;
        }

        public void MakeLong()
        {
            mRect.Width = mDefaultWidth * 1.35f;
        }

        public void MakeShort()
        {
            mRect.Width = mDefaultWidth * 0.75f;
        }

        // value from -1 (left) to +1 (right) for angle control :contentReference[oaicite:16]{index=16}
        public float GetHitFactor(float ballCenterX)
        {
            float paddleCenter = mRect.X + mRect.Width / 2f;
            float diff = ballCenterX - paddleCenter;
            return diff / (mRect.Width / 2f);
        }

        public void Draw(Graphics g, int x, int y)
        {
            using (SolidBrush b = new SolidBrush(Color.White))
                g.FillRectangle(b, mRect);

            using (Pen p = new Pen(Color.Black))
                g.DrawRectangle(p, mRect.X, mRect.Y, mRect.Width, mRect.Height);
        }
    }
}

