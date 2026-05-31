using System;
using System.Drawing;

namespace Breakout
{
    //*************************************************************
    //Enums
    //*************************************************************
    public enum MenuStates
    {
        Start,
        Instructions,
        Playing,
        GameOver
    }

    internal class Menu
    {
        //*************************************************************
        //Fields
        //*************************************************************
        private MenuStates mMenuState;
        private int mHelpPage;

        //*************************************************************
        //Constructor
        //*************************************************************
        public Menu()
        {
            mMenuState = MenuStates.Start;
            mHelpPage = 0;
        }

        //*************************************************************
        //Properties
        //*************************************************************
        public MenuStates State
        {
            get { return mMenuState; }
            set { mMenuState = value; }
        }

        public int HelpPage
        {
            get { return mHelpPage; }
            set { mHelpPage = value; }
        }

        //*************************************************************
        //Methods
        //*************************************************************

        //since everything must be drawn, instead of panels draw it on screen
        public void Draw(Graphics g, int w, int h, int score, int level)
        {
            using (Font title = new Font("Arial", 40, FontStyle.Bold))
            using (Font body = new Font("Arial", 16, FontStyle.Bold))
            using (Brush b = new SolidBrush(Color.White))
            {
                

                // START MENU 
                if (mMenuState == MenuStates.Start)
                {
                    // Title
                    DrawCentered(g, "BREAKOUT", title, b, w, 100);
                    DrawCentered(g, "ENTER = Start", body, b, w, 260);
                    DrawCentered(g, "I = Instructions / Controls", body, b, w, 295);
                    DrawCentered(g, "ESC = Exit", body, b, w, 330);
                }

                //  INSTRUCTIONS
                else if (mMenuState == MenuStates.Instructions)
                {
                    using (Font header = new Font("Arial", 26, FontStyle.Bold))
                    using (Font small = new Font("Arial", 14, FontStyle.Bold))
                    {
                        DrawCentered(g, "INSTRUCTIONS", header, b, w, 60);
                        DrawCentered(g, "LEFT / RIGHT = Change Page", small, b, w, 110);
                        DrawCentered(g, "ESC = Back to Menu", small, b, w, 135);

                        //  PAGE 0: CONTROLS 
                        if (mHelpPage == 0)
                        {
                            DrawCentered(g, "Controls", small, b, w, 200);
                            DrawCentered(g, "LEFT / RIGHT = Move Paddle", small, b, w, 235);
                            DrawCentered(g, "ENTER = Start Game", small, b, w, 265);
                            DrawCentered(g, "ESC = Exit / Back", small, b, w, 295);

                            DrawCentered(g, "Goal", small, b, w, 350);
                            DrawCentered(g, "Break all bricks to clear a screen.", small, b, w, 380);
                            DrawCentered(g, "Clear 2 screens to advance the level.", small, b, w, 410);
                            DrawCentered(g, "You have 3 lives.", small, b, w, 440);
                        }

                        // PAGE 1: LEVEL 1 RULES 
                        else if (mHelpPage == 1)
                        {
                            DrawCentered(g, "Level 1 Rules", small, b, w, 200);
                            DrawCentered(g, "Brick colors (bottom → top):", small, b, w, 235);
                            DrawCentered(g, "Yellow, Green, Orange, Red", small, b, w, 265);
                            DrawCentered(g, "Points: 1, 3, 5, 7", small, b, w, 295);

                            DrawCentered(g, "Ball speeds up after 4 and 12 hits", small, b, w, 345);
                            DrawCentered(g, "Ball speeds up first time hitting", small, b, w, 375);
                            DrawCentered(g, "Orange and Red rows", small, b, w, 405);

                            DrawCentered(g, "Paddle Shrink Rule", small, b, w, 455);
                            DrawCentered(g, "After breaking a red brick,", small, b, w, 485);
                            DrawCentered(g, "when the ball hits the top wall,", small, b, w, 515);
                            DrawCentered(g, "the paddle shrinks to half size.", small, b, w, 545);
                        }

                        // PAGE 2: LEVEL 2 POWER-UPS
                        else if (mHelpPage == 2)
                        {
                            DrawCentered(g, "Level 2: Power-Ups & Power-Downs", small, b, w, 200);

                            float ix = w / 2f - 260;     // left start for icons
                            float textX = ix + 70;       // text starts to the right of icons
                            float y = 250;               // starting y

                            // instant bricks
                            DrawIconBrick(g, ix, y, Color.DeepSkyBlue, ">>");
                            g.DrawString("Instant speed-up brick", small, b, textX, y - 2);

                            y += 40;
                            DrawIconBrick(g, ix, y, Color.MediumPurple, "<<");
                            g.DrawString("Instant slow-down brick", small, b, textX, y - 2);

                            // drops header
                            y += 60;
                            g.DrawString("Drop bricks (falling circles):", small, b, ix, y);

                            y += 40;
                            DrawIconBrick(g, ix, y, Color.LimeGreen, "L");
                            g.DrawString("Drop: Paddle becomes longer", small, b, textX, y - 2);

                            y += 40;
                            DrawIconBrick(g, ix, y, Color.HotPink, "S");
                            g.DrawString("Drop: Paddle becomes shorter", small, b, textX, y - 2);

                            y += 40;
                            DrawIconBrick(g, ix, y, Color.DeepSkyBlue, "F");
                            g.DrawString("Drop: Ball gets faster", small, b, textX, y - 2);

                            y += 40;
                            DrawIconBrick(g, ix, y, Color.MediumPurple, "W");
                            g.DrawString("Drop: Ball gets slower", small, b, textX, y - 2);
                        }


                        //  PAGE 3: LEVEL 3 
                        else
                        {
                            DrawCentered(g, "Level 3: Obstacles", small, b, w, 200);

                            DrawIconBrick(g, w / 2f - 22, 240, Color.Gray, "X");

                            DrawCentered(g, "Gray bricks are indestructible.", small, b, w, 275);
                            DrawCentered(g, "They never break.", small, b, w, 305);
                            DrawCentered(g, "You must aim around them.", small, b, w, 335);

                            DrawCentered(g, "Angle Control", small, b, w, 385);
                            DrawCentered(g, "Left side = Ball goes left", small, b, w, 415);
                            DrawCentered(g, "Center = Mostly straight up", small, b, w, 445);
                            DrawCentered(g, "Right side = Ball goes right", small, b, w, 475);
                        }

                        DrawCentered(g, $"Page {mHelpPage + 1}/4", small, b, w, h - 35);

                    }
                }

                // GAME OVER 
                else if (mMenuState == MenuStates.GameOver)
                {
                    DrawCentered(g, "BREAKOUT", title, b, w, 100);
                    DrawCentered(g, "GAME OVER", body, b, w, 260);
                    DrawCentered(g, $"Final Score: {score}", body, b, w, 300);
                    DrawCentered(g, "ENTER = Restart", body, b, w, 350);
                    DrawCentered(g, "ESC = Exit", body, b, w, 385);
                }
            }
        }

        //*************************************************************
        //Helpers
        //*************************************************************
        private void DrawCentered(Graphics g, string text, Font f, Brush b, int w, float y)
        {
            SizeF s = g.MeasureString(text, f);
            g.DrawString(text, f, b, (w - s.Width) / 2f, y);
        }

        private void DrawIconBrick(Graphics g, float x, float y, Color color, string text)
        {
            RectangleF r = new RectangleF(x, y, 44, 18);

            using (SolidBrush b = new SolidBrush(color))
                g.FillRectangle(b, r);

            g.DrawRectangle(Pens.Black, r.X, r.Y, r.Width, r.Height);

            using (Font f = new Font("Arial", 9, FontStyle.Bold))
            using (Brush wb = new SolidBrush(Color.White))
                g.DrawString(text, f, wb, r.X + 4, r.Y + 1);
        }
    }
}
