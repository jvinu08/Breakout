using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

//Jeremiah Vinu 
//January 16, 2025
// Breakout Game Project

//*************************************************************
// Form1 (Main Game Controller)
// Controls the game loop, timer, input,
// collisions, scoring, lives, levels, and drawing.
//*************************************************************


namespace Breakout
{
    public partial class Form1 : Form
    {
        //*************************************************************
        //Fields
        //*************************************************************
        private Timer mTimer;

        private Menu mMenu;
        private Ball mBall;
        private Paddle mPaddle;
        private Bricks mBricks;

        private bool mLeftDown;
        private bool mRightDown;

        private int mScore;
        private int mLives;

        private int mLevel;
        private int mScreensCleared; // “two screens of bricks” 

        // speed rules counters
        private int mHitCount;
        private bool mTouchedOrange;
        private bool mTouchedRed;

        // shrink rule: AFTER red row broken AND ball hits upper wall
        private bool mRedRowBroken;
        private bool mPaddleShrunk;
        private bool mBrokeThroughRed;


        // simple drop-items (level 2+ specials) 
        private struct DropItem
        {
            public RectangleF Rect;
            public BrickTypes Type;
            public float SpeedY;
        }
        private List<DropItem> mDrops;

        //*************************************************************
        //Constructor
        //*************************************************************
        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;

            mMenu = new Menu();
            mBricks = new Bricks();
            mBall = new Ball(ClientSize.Width / 2f, ClientSize.Height - 140f, 8f);
            mPaddle = new Paddle(ClientSize.Width / 2f - 70f, ClientSize.Height - 60f, 140f, 14f);

            mDrops = new List<DropItem>();

            ResetGame();

            mTimer = new Timer();
            mTimer.Interval = 10;
            mTimer.Tick += TickGame;
            mTimer.Start();

            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
        }

        //*************************************************************
        //Game Setup
        //*************************************************************
        private void ResetGame()
        {
            mScore = 0;
            mLives = 3; // “three turns” 

            mLevel = 1;
            mScreensCleared = 0;

            StartLevel();
            mMenu.State = MenuStates.Start;
        }

        private void StartLevel()
        {
            mBricks.CreateLevel(mLevel, ClientSize.Width, 0);
            ResetRound();

            mHitCount = 0;
            mTouchedOrange = false;
            mTouchedRed = false;

            mRedRowBroken = false;
            mPaddleShrunk = false;
            mBrokeThroughRed = false;


            mDrops.Clear();
        }

        private void ResetRound()
        {
            // reset paddle to normal size each life / round
            mPaddle.ResetSize();

            // also clear “shrunk this round” so the rule can happen again if needed
            mPaddleShrunk = false;

            // reset ball + paddle position
            mBall.Reset(ClientSize.Width / 2f, ClientSize.Height - 140f);
            mPaddle.SetPosition(ClientSize.Width / 2f - mPaddle.Rect.Width / 2f, ClientSize.Height - 60f);
        }


        //*************************************************************
        //Main Loop
        //*************************************************************
        private void TickGame(object sender, EventArgs e)
        {
            if (mMenu.State != MenuStates.Playing)
            {
                Invalidate();
                return;
            }

            // paddle input
            if (mLeftDown) mPaddle.MoveLeft();
            if (mRightDown) mPaddle.MoveRight();
            mPaddle.KeepInBounds(ClientSize.Width);

            // move ball
            mBall.Update();

            // side walls
            if (mBall.Rect.Left <= 0 || mBall.Rect.Right >= ClientSize.Width)
                mBall.BounceX();

            // top wall (shrink rule happens here)
            if (mBall.Rect.Top <= 0)
            {
                // Only shrink after we have broken through red brick(s)
                if (mBrokeThroughRed && !mPaddleShrunk)
                {
                    mPaddle.ShrinkHalf();
                    mPaddleShrunk = true;
                }

                mBall.BounceY();
            }


            // Check if ball missed paddle (bottom of screen)
            // Player loses a life when this happens
            // bottom miss = lose life/turn
            if (mBall.Rect.Top > ClientSize.Height)
            {
                mLives--;
                if (mLives <= 0)
                {
                    mMenu.State = MenuStates.GameOver;
                }
                else
                {
                    ResetRound();
                }
                Invalidate();
                return;
            }

            // paddle collision + angle control 
            if (mBall.Rect.IntersectsWith(mPaddle.Rect))
            {
                // push ball above paddle to prevent “sticking”
                // (simple + reliable)
                mBall.BounceY();
                mBall.SetFromPaddleHit(mPaddle.GetHitFactor(mBall.X));

                RegisterHitForSpeedRules();
            }

            // brick collision
            BrickTypes hitType;
            Color hitColor;
            RectangleF hitRect;
            bool destroyed;

            int gained = mBricks.CheckBallCollision(mBall.Rect, out hitType, out hitColor, out hitRect, out destroyed);
            if (hitType != BrickTypes.None)
            {
                // basic bounce: decide which axis to flip (simple)
                // If ball is more “side” hit, flip X, else flip Y
                float ballCenterX = mBall.Rect.X + mBall.Rect.Width / 2f;
                if (ballCenterX < hitRect.Left || ballCenterX > hitRect.Right) mBall.BounceX();
                else mBall.BounceY();

                if (gained > 0) mScore += gained;

                RegisterHitForSpeedRules();

                // speed rules for orange/red contact 
                if (hitColor == Color.Orange && !mTouchedOrange)
                {
                    mBall.SpeedUp(1.12f);
                    mTouchedOrange = true;
                }
                if (hitColor == Color.Red && !mTouchedRed)
                {
                    mBall.SpeedUp(1.12f);
                    mTouchedRed = true;
                }
                // If we destroyed a red brick, we have "broken through" the red row
                if (destroyed && hitColor == Color.Red)
                    mBrokeThroughRed = true;


                // “red row broken” tracking for shrink rule 
                if (!mRedRowBroken && mBricks.RedRowCleared())
                    mRedRowBroken = true;

                // special brick effects (Level 2+)
                if (hitType == BrickTypes.SpeedUp) mBall.SpeedUp(1.10f);
                if (hitType == BrickTypes.SlowDown) mBall.SlowDown(0.90f);

                // drops (only if brick got destroyed)
                if (destroyed && (hitType == BrickTypes.DropLong || hitType == BrickTypes.DropShort ||
                    hitType == BrickTypes.DropSpeedUp || hitType == BrickTypes.DropSlowDown))
                {
                    DropItem d = new DropItem();
                    d.Rect = new RectangleF(hitRect.X + hitRect.Width / 2f - 10, hitRect.Y + hitRect.Height / 2f - 10, 20, 20);
                    d.Type = hitType;
                    d.SpeedY = 6f;
                    mDrops.Add(d);
                }
            }
            // Always update red-row status (so shrink rule can trigger later)
            if (!mRedRowBroken && mBricks.RedRowCleared())
                mRedRowBroken = true;


            // update drops
            for (int i = mDrops.Count - 1; i >= 0; i--)
            {
                DropItem d = mDrops[i];
                d.Rect = new RectangleF(d.Rect.X, d.Rect.Y + d.SpeedY, d.Rect.Width, d.Rect.Height);
                mDrops[i] = d;

                if (d.Rect.IntersectsWith(mPaddle.Rect))
                {
                    // apply
                    if (d.Type == BrickTypes.DropLong) mPaddle.MakeLong();
                    if (d.Type == BrickTypes.DropShort) mPaddle.MakeShort();
                    if (d.Type == BrickTypes.DropSpeedUp) mBall.SpeedUp(1.10f);
                    if (d.Type == BrickTypes.DropSlowDown) mBall.SlowDown(0.90f);

                    mDrops.RemoveAt(i);
                }
                else if (d.Rect.Top > ClientSize.Height)
                {
                    mDrops.RemoveAt(i);
                }
            }

            // cleared screen
            if (mBricks.AllCleared())
            {
                mScreensCleared++;

                // If you cleared 2 screens, advance the level
                if (mScreensCleared >= 2)
                {
                    mScreensCleared = 0;  // back to Screen 1/2
                    mLevel++;             // Level 2, 3, 4...
                }

                StartLevel();
            }


            Invalidate();
        }

        private void RegisterHitForSpeedRules()
        {
            // speed increases after 4 hits and after 12 hits
            mHitCount++;
            if (mHitCount == 4) mBall.SpeedUp(1.10f);
            if (mHitCount == 12) mBall.SpeedUp(1.10f);
        }

        //*************************************************************
        //Drawing
        //*************************************************************
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            //  BACKGROUND FIRST (so it doesn't cover the game) 
            using (LinearGradientBrush bg = new LinearGradientBrush(
                ClientRectangle,
                Color.FromArgb(20, 30, 80),
                Color.FromArgb(120, 30, 160),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(bg, ClientRectangle);
            }

            using (SolidBrush star = new SolidBrush(Color.FromArgb(70, Color.White)))
            {
                for (int i = 0; i < 80; i++)
                    g.FillEllipse(star, (i * 97) % ClientSize.Width, (i * 53) % ClientSize.Height, 2, 2);
            }

            //  MENU
            if (mMenu.State == MenuStates.Start ||  mMenu.State == MenuStates.GameOver ||  mMenu.State == MenuStates.Instructions)
            {
                mMenu.Draw(g, ClientSize.Width, ClientSize.Height, mScore, mLevel);
                return;
            }



            // DRAW GAME OBJECTS
            mBricks.Draw(g, 0, 0);
            mPaddle.Draw(g, 0, 0);
            mBall.Draw(g, 0, 0);

            //  DROPS
            foreach (DropItem d in mDrops)
            {
                Brush b = Brushes.White;
                if (d.Type == BrickTypes.DropLong) b = Brushes.LimeGreen;
                if (d.Type == BrickTypes.DropShort) b = Brushes.HotPink;
                if (d.Type == BrickTypes.DropSpeedUp) b = Brushes.DeepSkyBlue;
                if (d.Type == BrickTypes.DropSlowDown) b = Brushes.MediumPurple;

                g.FillEllipse(b, d.Rect);
                g.DrawEllipse(Pens.Black, d.Rect);
            }

            // HUD 
            using (Font f = new Font("Arial", 14, FontStyle.Bold))
            using (Brush b = new SolidBrush(Color.White))
            {
                g.DrawString("Press I for help", f, b, ClientSize.Width - 170, ClientSize.Height - 30);
                g.DrawString($"Score: {mScore}", f, b, 12, ClientSize.Height - 30);
                g.DrawString($"Lives: {mLives}", f, b, 160, ClientSize.Height - 30);
                g.DrawString($"Level: {mLevel}", f, b, 300, ClientSize.Height - 30);
                g.DrawString($"Screen: {mScreensCleared + 1}/2", f, b, 420, ClientSize.Height - 30);
            }
        }


        //*************************************************************
        //Input
        //*************************************************************
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //  INSTRUCTIONS STATE CONTROLS 
            if (mMenu.State == MenuStates.Instructions)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    mMenu.State = MenuStates.Start;
                    return;
                }

                if (e.KeyCode == Keys.Right)
                    mMenu.HelpPage = Math.Min(mMenu.HelpPage + 1, 3);

                if (e.KeyCode == Keys.Left)
                    mMenu.HelpPage = Math.Max(mMenu.HelpPage - 1, 0);

                Invalidate();
                return;
            }
            if (e.KeyCode == Keys.Left) mLeftDown = true;
            if (e.KeyCode == Keys.Right) mRightDown = true;

          
            

            if (e.KeyCode == Keys.Escape)
            {
                Close();
                return;
            }
            // Open instructions during gameplay too
            if (e.KeyCode == Keys.I)
            {
                mMenu.HelpPage = 0;
                mMenu.State = MenuStates.Instructions;
                Invalidate();
                return;
            }


            if (mMenu.State == MenuStates.Start)
            {
                if (e.KeyCode == Keys.Enter)
                    mMenu.State = MenuStates.Playing;

                if (e.KeyCode == Keys.I)
                {
                    mMenu.HelpPage = 0;
                    mMenu.State = MenuStates.Instructions;
                }
            }
            else if (mMenu.State == MenuStates.GameOver)
            {
                if (e.KeyCode == Keys.Enter)
                    ResetGame();
            }

        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) mLeftDown = false;
            if (e.KeyCode == Keys.Right) mRightDown = false;
        }
    }
}

