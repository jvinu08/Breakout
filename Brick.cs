using System;
using System.Drawing;
using System.Windows.Forms;

//*************************************************************
// Brick Class
// Represents a single brick in the Breakout game.
// Stores its position, colour, type, point value,
// and handles hit detection and destruction.
//*************************************************************


namespace Breakout
{
    //*************************************************************
    //Enums
    //*************************************************************
    // template had only None :contentReference[oaicite:9]{index=9}
    public enum BrickTypes
    {
        None,
        Regular,
        Strong,         // needs multiple hits
        Indestructible, // never disappears
        SpeedUp,        // changes ball speed
        SlowDown,
        DropLong,       // drops item that makes paddle longer
        DropShort,      // drops item that makes paddle shorter
        DropSpeedUp,
        DropSlowDown
    }

    internal class Brick
    {
        //*************************************************************
        //Fields
        //*************************************************************
        private BrickTypes mBrickType;
        private Color mColor;
        private RectangleF mRect;
        private bool mAlive;
        private int mPoints;
        private int mHitsLeft; // for Strong bricks

        //*************************************************************
        //Constructors
        //*************************************************************
        public Brick()
        {
            mBrickType = BrickTypes.None;
            mColor = Color.Transparent;
            mRect = new RectangleF(0, 0, 0, 0);
            mAlive = false;
            mPoints = 0;
            mHitsLeft = 1;
        }

        public Brick(RectangleF rect, Color color, int points, BrickTypes type, int hitsLeft = 1)
        {
            mRect = rect;
            mColor = color;
            mPoints = points;
            mBrickType = type;
            mAlive = (type != BrickTypes.None);
            mHitsLeft = hitsLeft;
        }

        //*************************************************************
        //Properties
        //*************************************************************
        public RectangleF Rect { get { return mRect; } }
        public bool Alive { get { return mAlive; } }
        public BrickTypes Type { get { return mBrickType; } }
        public Color Color { get { return mColor; } }
        public int Points { get { return mPoints; } }

        //*************************************************************
        //Methods
        //*************************************************************
        public bool Intersects(RectangleF other)
        {
            return mAlive && mRect.IntersectsWith(other);
        }

        // returns points earned; also tells caller if brick was destroyed
        public int Hit(out bool destroyed)
        {
            destroyed = false;

            if (!mAlive) return 0;

            if (mBrickType == BrickTypes.Indestructible)
            {
                return 0;
            }

            if (mBrickType == BrickTypes.Strong)
            {
                mHitsLeft--;
                if (mHitsLeft <= 0)
                {
                    mAlive = false;
                    destroyed = true;
                    return mPoints;
                }
                else
                {
                    // visual feedback: darken a bit
                    mColor = ControlPaint.Dark(mColor);
                    return 0;
                }
            }

            // regular / speed bricks / drop bricks
            mAlive = false;
            destroyed = true;
            return mPoints;
        }

        public void Draw(Graphics g, int x, int y)
        {
            if (!mAlive) return;

            using (SolidBrush b = new SolidBrush(mColor))
                g.FillRectangle(b, mRect);

            using (Pen p = new Pen(Color.Black))
                g.DrawRectangle(p, mRect.X, mRect.Y, mRect.Width, mRect.Height);

            // small marking for special bricks (simple, still “course level”)
            if (mBrickType != BrickTypes.Regular)
            {
                using (Font f = new Font("Arial", 10, FontStyle.Bold))
                using (Brush wb = new SolidBrush(Color.White))
                {
                    string mark = "?";
                    if (mBrickType == BrickTypes.Strong) mark = "2";
                    else if (mBrickType == BrickTypes.Indestructible) mark = "X";
                    else if (mBrickType == BrickTypes.SpeedUp) mark = ">>";
                    else if (mBrickType == BrickTypes.SlowDown) mark = "<<";
                    else if (mBrickType == BrickTypes.DropLong) mark = "L";
                    else if (mBrickType == BrickTypes.DropShort) mark = "S";
                    else if (mBrickType == BrickTypes.DropSpeedUp) mark = "F";
                    else if (mBrickType == BrickTypes.DropSlowDown) mark = "W";

                    g.DrawString(mark, f, wb, mRect.X + 4, mRect.Y + 2);
                }
            }

        }
    }
}

