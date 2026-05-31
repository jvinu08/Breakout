using System;
using System.Drawing;
using System.Windows;

//*************************************************************
// Ball Class
// Represents the ball in the game.
// Handles movement, velocity, collisions with
// walls, bricks, and the paddle.
//*************************************************************


namespace Breakout
{
    internal class Ball
    {
        //*************************************************************
        //Small vector struct (keeps it simple + works in WinForms)
        //*************************************************************
        internal struct Vec2
        {
            public float X;
            public float Y;

            public Vec2(float x, float y) { X = x; Y = y; }
        }

        //*************************************************************
        //Fields
        //*************************************************************
        private Vec2 mPosition;
        private Vec2 mVelocity;
        private float mRadius;

        //*************************************************************
        //Constructors
        //*************************************************************
        public Ball()
        {
            mPosition = new Vec2(200, 200);
            mVelocity = new Vec2(3.5f, -3.5f);
            mRadius = 8f;
        }

        public Ball(float x, float y, float radius)
        {
            mPosition = new Vec2(x, y);
            mVelocity = new Vec2(3.5f, -3.5f);
            mRadius = radius;
        }

        //*************************************************************
        //Properties
        //*************************************************************
        public float X { get { return mPosition.X; } }
        public float Y { get { return mPosition.Y; } }
        public float Radius { get { return mRadius; } }

        public RectangleF Rect
        {
            get { return new RectangleF(mPosition.X - mRadius, mPosition.Y - mRadius, mRadius * 2, mRadius * 2); }
        }

        //*************************************************************
        //Methods
        //*************************************************************
        public void Reset(float x, float y)
        {
            mPosition = new Vec2(x, y);
            mVelocity = new Vec2(3.5f, -3.5f);
        }

        public void Update()
        {
            mPosition.X += mVelocity.X;
            mPosition.Y += mVelocity.Y;
        }

        public void BounceX() { mVelocity.X = -mVelocity.X; }
        public void BounceY() { mVelocity.Y = -mVelocity.Y; }

        public void SpeedUp(float factor)
        {
            mVelocity.X *= factor;
            mVelocity.Y *= factor;
        }

        public void SlowDown(float factor)
        {
            mVelocity.X *= factor;
            mVelocity.Y *= factor;
        }

        // paddle angle control :contentReference[oaicite:18]{index=18}
        public void SetFromPaddleHit(float hitFactor)
        {
            // Keep the same speed, only change direction (angle).
            // This prevents "speed boost" when hitting paddle edges.

            // 1) current speed magnitude
            float speed = (float)Math.Sqrt(mVelocity.X * mVelocity.X + mVelocity.Y * mVelocity.Y);

            // 2) pick a new direction based on hitFactor
            // hitFactor is -1..1, clamp just in case
            if (hitFactor < -1f) hitFactor = -1f;
            if (hitFactor > 1f) hitFactor = 1f;

            // max horizontal ratio (how sideways it can go)
            float maxX = 0.85f; // 0 = straight up, 1 = super sideways
            float xRatio = hitFactor * maxX;

            // yRatio chosen so x^2 + y^2 = 1
            float yRatio = (float)Math.Sqrt(1f - xRatio * xRatio);

            // 3) apply velocity with same speed; y goes upward (negative)
            mVelocity.X = speed * xRatio;
            mVelocity.Y = -speed * yRatio;

            // 4) stop perfectly vertical “boring” shots
            if (Math.Abs(mVelocity.X) < 1.2f)
                mVelocity.X = (mVelocity.X < 0) ? -1.2f : 1.2f;
        }


        public void Draw(Graphics g, int x, int y)
        {
            using (SolidBrush b = new SolidBrush(Color.White))
                g.FillEllipse(b, mPosition.X - mRadius, mPosition.Y - mRadius, mRadius * 2, mRadius * 2);

            using (Pen p = new Pen(Color.Black))
                g.DrawEllipse(p, mPosition.X - mRadius, mPosition.Y - mRadius, mRadius * 2, mRadius * 2);
        }
    }
}

