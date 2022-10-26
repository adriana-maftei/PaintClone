using System.Drawing.Imaging;

namespace PaintClone
{
    public partial class PAINT : Form
    {
        public PAINT()
        {
            InitializeComponent();

            this.Width = 950;
            this.Height = 800;
            bm = new Bitmap(pic.Width, pic.Height);
            gfx = Graphics.FromImage(bm);
            gfx.Clear(Color.White);
            pic.Image = bm;
        }

        private Bitmap bm;
        private Graphics gfx;
        private Pen pen = new Pen(Color.Black, 1);
        private Pen erase = new Pen(Color.White, 50);
        private Point px, py;

        private bool paint = false;
        private int index;
        private int x, y, sx, sy, cx, cy; //limits and coordinates of drawing

        private ColorDialog cd = new ColorDialog();
        private Color color;

        #region Mouse events
        private void pic_MouseDown(object sender, MouseEventArgs e)
        {
            paint = true;
            py = e.Location;

            cx = e.X;
            cy = e.Y;
        }

        private void pic_MouseMove(object sender, MouseEventArgs e)
        {
            if (paint)
            {
                if (index == 1)
                {
                    px = e.Location;
                    gfx.DrawLine(pen, px, py);
                    py = px;
                }
                if (index == 2)
                {
                    px = e.Location;
                    gfx.DrawLine(erase, px, py);
                    py = px;
                }
            }
            pic.Refresh();

            x = e.X;
            y = e.Y;
            sx = e.X - cx;
            sy = e.Y - cy;
        }

        private void pic_MouseUp(object sender, MouseEventArgs e)
        {
            paint = false;
            sx = x - cx;
            sy = y - cy;

            if (index == 3)
                gfx.DrawEllipse(pen, cx, cy, sx, sy);
            if (index == 4)
                gfx.DrawRectangle(pen, cx, cy, sx, sy);
            if (index == 5)
                gfx.DrawLine(pen, cx, cy, x, y);
        }

        private void pic_Paint(object sender, PaintEventArgs e)
        {
            Graphics gfx = e.Graphics;
            if (paint) //instant display drawing on screen
            {
                if (index == 3)
                    gfx.DrawEllipse(pen, cx, cy, sx, sy);
                if (index == 4)
                    gfx.DrawRectangle(pen, cx, cy, sx, sy);
                if (index == 5)
                    gfx.DrawLine(pen, cx, cy, x, y);
            }
        }

        private void color_picker_MouseClick(object sender, MouseEventArgs e)
        {
            Point point = setPoint(color_picker, e.Location);
            pic_color.BackColor = ((Bitmap)color_picker.Image).GetPixel(point.X, point.Y);
            color = pic_color.BackColor;
            pen.Color = pic_color.BackColor;
        }

        private void pic_MouseClick(object sender, MouseEventArgs e)
        {
            if(index == 6)
            {
                Point point = setPoint(pic, e.Location);
                Fill(bm, point.X, point.Y, color);
            }
        }
        #endregion

        #region Indexes of buttons
        private void btn_pen_Click(object sender, EventArgs e) => index = 1;

        private void btn_eraser_Click(object sender, EventArgs e) => index = 2;

        private void btn_circle_Click(object sender, EventArgs e) => index = 3;

        private void btn_square_Click(object sender, EventArgs e) => index = 4;

        private void btn_line_Click(object sender, EventArgs e) => index = 5;
        #endregion

        #region Color picker
        private void btn_color_Click(object sender, EventArgs e)
        {
            cd.ShowDialog(); //instantiate a color dialog box 
            color = cd.Color; //pick color
            pic_color.BackColor = cd.Color; //display current color picked
            pen.Color = cd.Color; //change pen color to picked color
        }

        private static Point setPoint(PictureBox pb, Point pt)
        {
            float px = 1f * pb.Image.Width / pb.Width;
            float py = 1f * pb.Image.Height / pb.Height;
            return new Point((int)(pt.X * px), (int)(pt.Y * py));
        }
        #endregion

        #region Color filler
        private void btn_fill_Click(object sender, EventArgs e) => index = 6;

        private void Validate(Bitmap bm, Stack<Point> sp, int x, int y, Color oldColor, Color newColor)
        {
            Color cx = bm.GetPixel(x, y);
            if (cx == oldColor)
            {
                sp.Push(new Point(x, y));
                bm.SetPixel(x, y, newColor);
            }
        }

        public void Fill(Bitmap bm, int x, int y, Color new_color)
        {
            Color old_color = bm.GetPixel(x, y);
            Stack<Point> pixel = new Stack<Point>(); 
            pixel.Push(new Point(x, y));
            bm.SetPixel(x, y, new_color);
            if (old_color == new_color) return;

            while (pixel.Count > 0) //fill new color from the clicked point until fill completely
            {
                Point pt = pixel.Pop();
                if (pt.X > 0 && pt.Y > 0 && pt.X < bm.Width-1 && pt.Y < bm.Height - 1)
                {
                    Validate(bm, pixel, pt.X - 1, pt.Y, old_color, new_color);
                    Validate(bm, pixel, pt.X, pt.Y - 1, old_color, new_color);
                    Validate(bm, pixel, pt.X + 1, pt.Y, old_color, new_color);
                    Validate(bm, pixel, pt.X, pt.Y + 1, old_color, new_color);
                }
            }

        }
        #endregion

        private void btn_clear_Click(object sender, EventArgs e)
        {
            gfx.Clear(Color.White);
            pic.Image = bm;
            index = 0;
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            var sf = new SaveFileDialog();
            sf.Filter = "Image(*.jpg)|*.jpg|(*.*|*.*";

            if (sf.ShowDialog() == DialogResult.OK)
            {
                Bitmap btm = bm.Clone(new Rectangle(0, 0, pic.Width, pic.Height), bm.PixelFormat);
                btm.Save(sf.FileName, ImageFormat.Jpeg);
            }
        }
    }
}