namespace CaptureX
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using CaptureX;
    internal class CaptureForm : Form
    {
        private ChangingType ct;
        private string currPixel;
        private Rectangle currRect;
        private Bitmap currScr;
        private Bitmap currScrMask;
        private Size distance;
        private Bitmap infoPanelPic;
        private bool isChangingRect;
        private bool isDrawingRect;
        private bool isMovingRect;
        private bool isRectExist;
        private SolidBrush mask = new SolidBrush(Color.FromArgb(0x4d000000));
        private Point rectPos;

        public CaptureForm()
        {
            base.FormBorderStyle = FormBorderStyle.None;
            base.WindowState = FormWindowState.Maximized;
            base.TopMost = true;
            base.Load += new EventHandler(this.CaptureForm_Load);
            base.MouseDown += new MouseEventHandler(this.CaptureForm_MouseDown);
            base.MouseMove += new MouseEventHandler(this.CaptureForm_MouseMove);
            base.MouseUp += new MouseEventHandler(this.CaptureForm_MouseUp);
            base.MouseClick += new MouseEventHandler(this.CaptureForm_MouseClick);
            base.MouseDoubleClick += new MouseEventHandler(this.CaptureForm_MouseDoubleClick);
            base.KeyDown += new KeyEventHandler(this.CaptureForm_KeyDown);
            this.currScrMask = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(this.currScrMask);
            g.CopyFromScreen(0, 0, 0, 0, new Size(this.currScrMask.Width, this.currScrMask.Height));
            this.currScr = new Bitmap(this.currScrMask);
            //g.FillRectangle(this.mask, 0, 0, this.currScr.Width, this.currScr.Height);
            g.Dispose();
            this.BackgroundImage = this.currScrMask;

            //this.Cursor = Cursors.Cross;

            // 截图位置记忆
            if (StaticClass.Config.CurrRectRemember)
            {
                if (StaticClass.CurrRect.IsEmpty == false)
                {
                    this.currRect = StaticClass.CurrRect;
                    this.isRectExist = true;
                    //this.isMovingRect = true;
                }
                else
                {
                    this.isRectExist = false;
                }
            }
        }

        private void CaptureForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    this.currRect.X--;
                    break;

                case Keys.Up:
                    this.currRect.Y--;
                    break;

                case Keys.Right:
                    this.currRect.X++;
                    break;

                case Keys.Down:
                    this.currRect.Y++;
                    break;

                case Keys.Escape:
                    if (this.isRectExist)
                    {
                        this.Cursor = Cursors.Default;
                        this.isRectExist = false;
                        this.currRect = Rectangle.Empty;
                        this.RefreshScr();

                    }
                    else
                    {
                        base.DialogResult = DialogResult.Cancel;
                        base.Close();
                    }
                    break;

                case Keys.Return:
                    if (this.isRectExist)
                    {
                        this.CaptureReturn();
                    }
                    break;
            }
            this.RefreshScr();
        }

        private void CaptureForm_Load(object sender, EventArgs e)
        {
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            base.UpdateStyles();

        }

        private void CaptureForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (this.isRectExist)
                {
                    if (!this.IsPointInRectangle(e.Location, this.currRect))
                    {
                        this.isRectExist = false;
                        this.currRect = Rectangle.Empty;
                        this.RefreshScr();
                    }
                }
                else
                {
                    base.DialogResult = DialogResult.Cancel;
                    base.Close();
                }
            }
        }

        private void CaptureForm_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.CaptureReturn();
            }
        }

        private void CaptureForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {

                int[,] anchor = this.GetDraggableAnchorPostion();
                if (!this.isRectExist)
                {
                    this.isDrawingRect = true;
                    this.isRectExist = true;
                    this.rectPos = e.Location;
                }
                else if (this.IsPointInRectangle(e.Location, anchor[0, 0] - 2, anchor[0, 1] - 2, 4, 5))
                {
                    this.ct = ChangingType.Both;
                    this.isChangingRect = true;
                    this.rectPos = new Point(this.currRect.X + this.currRect.Width, this.currRect.Y + this.currRect.Height);
                }
                else if (this.IsPointInRectangle(e.Location, anchor[2, 0] - 2, anchor[2, 1] - 2, 4, 5))
                {
                    this.ct = ChangingType.Both;
                    this.isChangingRect = true;
                    this.rectPos = new Point(this.currRect.X, this.currRect.Y + this.currRect.Height);
                }
                else if (this.IsPointInRectangle(e.Location, anchor[5, 0] - 2, anchor[5, 1] - 2, 4, 5))
                {
                    this.ct = ChangingType.Both;
                    this.isChangingRect = true;
                    this.rectPos = new Point(this.currRect.X + this.currRect.Width, this.currRect.Y);
                }
                else if (this.IsPointInRectangle(e.Location, anchor[7, 0] - 2, anchor[7, 1] - 2, 4, 5))
                {
                    this.ct = ChangingType.Both;
                    this.isChangingRect = true;
                    this.rectPos = new Point(this.currRect.X, this.currRect.Y);
                }
                else if (this.IsPointInRectangle(e.Location, anchor[1, 0] - 2, anchor[1, 1] - 2, 4, 5))
                {
                    this.ct = ChangingType.Height;
                    this.isChangingRect = true;
                    this.rectPos = new Point(this.currRect.X, this.currRect.Y + this.currRect.Height);
                }
                else if (this.IsPointInRectangle(e.Location, anchor[3, 0] - 2, anchor[3, 1] - 2, 4, 5))
                {
                    this.ct = ChangingType.Width;
                    this.isChangingRect = true;
                    this.rectPos = new Point(this.currRect.X + this.currRect.Width, this.currRect.Y);
                }
                else if (this.IsPointInRectangle(e.Location, anchor[4, 0] - 2, anchor[4, 1] - 2, 4, 5))
                {
                    this.ct = ChangingType.Width;
                    this.isChangingRect = true;
                    this.rectPos = new Point(this.currRect.X, this.currRect.Y);
                }
                else if (this.IsPointInRectangle(e.Location, anchor[6, 0] - 2, anchor[6, 1] - 2, 4, 5))
                {
                    this.ct = ChangingType.Height;
                    this.isChangingRect = true;
                    this.rectPos = new Point(this.currRect.X, this.currRect.Y);
                }
                else if (this.IsPointInRectangle(e.Location, this.currRect))
                {
                    this.isMovingRect = true;
                    this.distance = ((Size) e.Location) - ((Size) this.currRect.Location);
                }
            }
        }

        private void CaptureForm_MouseMove(object sender, MouseEventArgs e)
        {
            Color pixelColor = this.currScr.GetPixel(e.X, e.Y);
            this.currPixel = string.Format("({0},{1},{2})", pixelColor.R, pixelColor.G, pixelColor.B);
            if (this.isRectExist && !this.isChangingRect)
            {
                int[,] dots = this.GetDraggableAnchorPostion();
                if (this.IsPointInRectangle(e.Location, new Rectangle(dots[0, 0] - 2, dots[0, 1] - 2, 4, 5)) || this.IsPointInRectangle(e.Location, new Rectangle(dots[7, 0] - 2, dots[7, 1] - 2, 4, 5)))
                {
                    this.Cursor = Cursors.SizeNWSE;
                }
                else if (this.IsPointInRectangle(e.Location, new Rectangle(dots[1, 0] - 2, dots[1, 1] - 2, 4, 5)) || this.IsPointInRectangle(e.Location, new Rectangle(dots[6, 0] - 2, dots[6, 1] - 2, 4, 5)))
                {
                    this.Cursor = Cursors.SizeNS;
                }
                else if (this.IsPointInRectangle(e.Location, new Rectangle(dots[2, 0] - 2, dots[2, 1] - 2, 4, 5)) || this.IsPointInRectangle(e.Location, new Rectangle(dots[5, 0] - 2, dots[5, 1] - 2, 4, 5)))
                {
                    this.Cursor = Cursors.SizeNESW;
                }
                else if (this.IsPointInRectangle(e.Location, new Rectangle(dots[3, 0] - 2, dots[3, 1] - 2, 4, 5)) || this.IsPointInRectangle(e.Location, new Rectangle(dots[4, 0] - 2, dots[4, 1] - 2, 4, 5)))
                {
                    this.Cursor = Cursors.SizeWE;
                }
                else if (this.IsPointInRectangle(e.Location, this.currRect))
                {
                    this.Cursor = Cursors.SizeAll;
                }
                else
                {
                    this.Cursor = Cursors.Default;
                }
            }
            if (e.Button == MouseButtons.Left)
            {
                if (this.isDrawingRect)
                {
                    this.currRect = this.GetCurrentRect(e);
                    //this.RefreshScr();
                }
                else if (this.isChangingRect)
                {
                    if (this.ct == ChangingType.Both)
                    {
                        this.currRect = this.GetCurrentRect(e);
                        //this.RefreshScr();
                    }
                    else if (this.ct == ChangingType.Height)
                    {
                        int rectH = Math.Abs((int) (e.Y - this.rectPos.Y));
                        int rectY = (e.Y < this.rectPos.Y) ? e.Y : this.rectPos.Y;
                        this.currRect = new Rectangle(this.currRect.X, rectY, this.currRect.Width, rectH);
                        //this.RefreshScr();
                    }
                    else if (this.ct == ChangingType.Width)
                    {
                        int rectW = Math.Abs((int) (e.X - this.rectPos.X));
                        int rectX = (e.X < this.rectPos.X) ? e.X : this.rectPos.X;
                        this.currRect = new Rectangle(rectX, this.currRect.Y, rectW, this.currRect.Height);
                        //this.RefreshScr();
                    }
                }
                else if (this.isMovingRect)
                {
                    Point p = e.Location - this.distance;
                    if (p.X < 0)
                    {
                        p.X = 0;
                    }
                    if (p.Y < 0)
                    {
                        p.Y = 0;
                    }
                    if (p.X > (this.currScr.Width - this.currRect.Width))
                    {
                        p.X = this.currScr.Width - this.currRect.Width;
                    }
                    if (p.Y > (this.currScr.Height - this.currRect.Height))
                    {
                        p.Y = this.currScr.Height - this.currRect.Height;
                    }
                    this.currRect = new Rectangle(p, this.currRect.Size);
                    //this.RefreshScr();
                }
            }
            
            this.RefreshScr();
           
        }

        private void CaptureForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (this.isDrawingRect)
                {
                    this.isDrawingRect = false;
                }
                else if (this.isChangingRect)
                {
                    this.isChangingRect = false;
                }
                else if (this.isMovingRect)
                {
                    this.isMovingRect = false;
                }
            }
        }

        protected void CaptureReturn()
        {
            Bitmap b = new Bitmap(this.currRect.Width, this.currRect.Height);
            Graphics g = Graphics.FromImage(b);
            g.DrawImage(this.currScr, new Rectangle(0, 0, this.currRect.Width, this.currRect.Height), this.currRect, GraphicsUnit.Pixel);
            g.Dispose();
            Clipboard.SetImage(b);
            b.Dispose();
            base.DialogResult = DialogResult.OK;
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.currScr.Dispose();
                this.currScrMask.Dispose();
                this.mask.Dispose();
                this.infoPanelPic.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 信息面板
        /// </summary>
        /// <param name="g"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="zone"></param>
        /// <param name="rgb"></param>
        protected void DrawInfoPanel(Graphics g, int x, int y, string zone, string rgb,Point m)
        {
            
            if (this.infoPanelPic == null)
            {
                this.infoPanelPic = new Bitmap(260, 100);
                Graphics g1 = Graphics.FromImage(this.infoPanelPic);
                g1.FillRectangle(this.mask, 0, 0, this.infoPanelPic.Width, this.infoPanelPic.Height);
                Font f = new Font("宋体", 10f);
                Font ff = new Font("宋体", 9f,FontStyle.Bold);
                Pen p = new Pen(Brushes.Orange, 1f);
                g1.DrawString("CaptureX", ff, Brushes.Orange, new PointF(108f, 8f));
                g1.DrawLine(p, new PointF(108f, 24f), new PointF(250f, 24f));
                g1.DrawString("大小:", f, Brushes.White, new PointF(108f, 32f));
                g1.DrawString("RGB:", f, Brushes.White, new PointF(108f, 53f));
                g1.DrawString("双击可完成截屏", f, Brushes.White, new PointF(108f, 78f));
                f.Dispose();
                g1.Dispose();
            }

            Bitmap bb = new Bitmap(100, 100);
            Graphics gg = Graphics.FromImage(bb);
            Pen pp = new Pen(Color.Black);
            if (this.isMovingRect)
            {
                gg.DrawImage(this.currScrMask, new Rectangle(0, 0, 100, 100), new Rectangle(this.currRect.Location.X - 15, this.currRect.Location.Y - 15, 30, 30), GraphicsUnit.Pixel);
            }
            else
            {
                gg.DrawImage(this.currScrMask, new Rectangle(0, 0, 100, 100), new Rectangle(m.X - 15, m.Y - 15, 30, 30), GraphicsUnit.Pixel);
            }
            gg.DrawRectangle(pp, new Rectangle(0, 0, 99, 99));
            gg.DrawLine(pp, new Point(0, 50), new Point(100, 50));
            gg.DrawLine(pp, new Point(50, 0), new Point(50, 100));
            gg.Dispose();

            Bitmap b = new Bitmap(this.infoPanelPic);
            Graphics g2 = Graphics.FromImage(b);
            Font f2 = new Font("宋体", 9f);
            g2.DrawString(zone, f2, Brushes.White, new PointF(142f, 32f));
            g2.DrawString(rgb, f2, Brushes.White, new PointF(138f, 53f));
            g.DrawImageUnscaled(b, x, y);
            g.DrawImageUnscaled(bb, x, y);
            f2.Dispose();
            b.Dispose();
            bb.Dispose();
            g2.Dispose();
        }

        protected void DrawRectangleWithAnchor(Graphics g, Color color, Rectangle rect)
        {
            this.DrawRectangleWithAnchor(g, color, rect.X, rect.Y, rect.Width, rect.Height);
        }

        protected void DrawRectangleWithAnchor(Graphics g, Color color, int x, int y, int width, int height)
        {
            Pen p = new Pen(color, 1f);
            g.DrawRectangle(p, x, y, width, height);
            p.Dispose();
            int[,] dots = new int[,] { { x, y }, { x + (width / 2), y }, { x + width, y }, { x, y + (height / 2) }, { x + width, y + (height / 2) }, { x, y + height }, { x + (width / 2), y + height }, { x + width, y + height } };
            SolidBrush sb = new SolidBrush(color);
            for (int i = 0; i < 8; i++)
            {
                g.FillRectangle(sb, dots[i, 0] - 2, dots[i, 1] - 2, 4, 5);
            }
            sb.Dispose();
        }

        protected Rectangle GetCurrentRect(MouseEventArgs e)
        {
            int rectW = Math.Abs((int) (e.X - this.rectPos.X));
            int rectH = Math.Abs((int) (e.Y - this.rectPos.Y));
            int rectX = (e.X < this.rectPos.X) ? e.X : this.rectPos.X;
            return new Rectangle(rectX, (e.Y < this.rectPos.Y) ? e.Y : this.rectPos.Y, rectW, rectH);
        }

        protected int[,] GetDraggableAnchorPostion()
        {
            int x = this.currRect.X;
            int y = this.currRect.Y;
            int width = this.currRect.Width;
            int height = this.currRect.Height;
            return new int[,] { { x, y }, { (x + (width / 2)), y }, { (x + width), y }, { x, (y + (height / 2)) }, { (x + width), (y + (height / 2)) }, { x, (y + height) }, { (x + (width / 2)), (y + height) }, { (x + width), (y + height) } };
        }

        protected bool IsPointInRectangle(Point p, Rectangle r)
        {
            return this.IsPointInRectangle(p, r.X, r.Y, r.Width, r.Height);
        }

        protected bool IsPointInRectangle(Point p, int x, int y, int width, int height)
        {
            return ((((p.X > x) && (p.Y > y)) && (p.X < (x + width))) && (p.Y < (y + height)));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            this.RefreshScr();
        }

        protected void RefreshScr()
        {
            Bitmap b = new Bitmap(this.currScrMask);
            Graphics g = Graphics.FromImage(b);
           
           
            if (this.isRectExist)
            {
                g.FillRectangle(this.mask, 0, 0, this.currScr.Width, this.currScr.Height);
            }
            g.DrawImage(this.currScr, this.currRect, this.currRect, GraphicsUnit.Pixel);
            // 线色 红
            this.DrawRectangleWithAnchor(g, Color.Red, this.currRect);

            
            //this.DrawInfoPanel(g, ((this.currRect.X + 160) > this.currScr.Width) ? (this.currRect.X - 160) : this.currRect.X, ((this.currRect.Y - 0x4c) > 0) ? (this.currRect.Y - 0x4c) : (this.currRect.Y + 6), this.currRect.Width.ToString() + "*" + this.currRect.Height.ToString(), this.currPixel);
            //鼠标位置
            Point m = Control.MousePosition;

            this.DrawInfoPanel(g, ((m.X + 260) > this.currScr.Width) ? (m.X - 260) : (m.X+20), ((m.Y + 110) > this.currScr.Height) ? (m.Y - 110) : (m.Y + 20), this.currRect.Width.ToString() + "*" + this.currRect.Height.ToString(), this.currPixel,m);

            

            StaticClass.CurrRect = this.currRect;
            
            g.Dispose();
            Graphics g2 = base.CreateGraphics();
            g2.DrawImageUnscaled(b, 0, 0);
            g2.Dispose();
            b.Dispose();
        }
    }
}

