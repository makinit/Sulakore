using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sulakore.Components
{
    [DesignerCategory("Code")]
    public class SKoreTabControl : TabControl
    {
        private bool _displayBoundary;
        [DefaultValue(false)]
        public bool DisplayBoundary
        {
            get { return _displayBoundary; }
            set { _displayBoundary = value; Invalidate(); }
        }

        [DefaultValue(typeof(Size), "95, 24")]
        public new Size ItemSize
        {
            get
            {
                if (Alignment == TabAlignment.Left || Alignment == TabAlignment.Right)
                    return new Size(base.ItemSize.Height, base.ItemSize.Width);
                return base.ItemSize;
            }
            set
            {
                bool isHorizontal = (Alignment == TabAlignment.Left || Alignment == TabAlignment.Right);

                if (isHorizontal) base.ItemSize = new Size(value.Height, value.Width);
                else base.ItemSize = value;

                Invalidate();
            }
        }

        private Color _skin = Color.SteelBlue;
        [DefaultValue(typeof(Color), "SteelBlue")]
        public Color Skin
        {
            get { return _skin; }
            set { _skin = value; Invalidate(); }
        }

        private Color _titleColor = Color.Black;
        [DefaultValue(typeof(Color), "Black")]
        public Color TitleColor
        {
            get { return _titleColor; }
            set { _titleColor = value; Invalidate(); }
        }

        private Color _backcolor = Color.White;
        [DefaultValue(typeof(Color), "White")]
        public Color Backcolor
        {
            get { return _backcolor; }
            set { _backcolor = value; Invalidate(); }
        }

        public SKoreTabControl()
        {
            SetStyle((ControlStyles)2050, true);
            DoubleBuffered = true;

            SizeMode = TabSizeMode.Fixed;
            ItemSize = new Size(95, 24);

            DrawMode = TabDrawMode.OwnerDrawFixed;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Backcolor);
            if (DisplayBoundary)
            {
                using (var pen = new Pen(Skin))
                    e.Graphics.DrawLine(pen, 0, Height - 1, Width - 1, Height - 1);
            }
            if (TabPages.Count < 1) { base.OnPaint(e); return; }

            Rectangle tabRectangle, tabTitle, tabGlow;
            using (var titleFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                for (int i = 0; i < TabPages.Count; i++)
                {
                    tabRectangle = GetTabRect(i);
                    switch (Alignment)
                    {
                        default:
                        case TabAlignment.Top:
                        {
                            tabTitle = new Rectangle(
                                tabRectangle.X + (i == 0 ? 2 : 0),
                                tabRectangle.Y + 2,
                                tabRectangle.Width - (i == 0 ? 4 : 2),
                                tabRectangle.Height - 4);

                            tabGlow = new Rectangle(tabTitle.X, (tabRectangle.Y + tabRectangle.Height) - 2, tabTitle.Width, 2);
                            break;
                        }
                        case TabAlignment.Bottom:
                        {
                            tabTitle = new Rectangle(
                                tabRectangle.X + (i == 0 ? 2 : 0),
                                tabRectangle.Y + 2,
                                tabRectangle.Width - (i == 0 ? 4 : 2),
                                tabRectangle.Height);

                            tabGlow = new Rectangle(tabTitle.X, tabRectangle.Y, tabTitle.Width, 2);
                            break;
                        }
                        case TabAlignment.Left:
                        {
                            titleFormat.Alignment = StringAlignment.Far;

                            tabTitle = new Rectangle(
                                tabRectangle.X - 2,
                                tabRectangle.Y + (i == 0 ? 2 : 0),
                                tabRectangle.Width - 2,
                                tabRectangle.Height - (i == 0 ? 4 : 2));

                            tabGlow = new Rectangle(tabTitle.X + tabRectangle.Width, tabTitle.Y, 2, tabTitle.Height);
                            break;
                        }
                        case TabAlignment.Right:
                        {
                            titleFormat.Alignment = StringAlignment.Near;

                            tabTitle = new Rectangle(
                                tabRectangle.X + 4,
                                tabRectangle.Y + (i == 0 ? 2 : 0),
                                tabRectangle.Width - 2,
                                tabRectangle.Height - (i == 0 ? 4 : 2));

                            tabGlow = new Rectangle(tabRectangle.X, tabTitle.Y, 2, tabTitle.Height);
                            break;
                        }
                    }

                    using (var solidBrush = new SolidBrush(SelectedIndex == i ? Skin : Color.Silver))
                        e.Graphics.FillRectangle(solidBrush, tabGlow);

                    using (var solidBrush = new SolidBrush(TitleColor))
                        e.Graphics.DrawString(TabPages[i].Text, Font, solidBrush, tabTitle, titleFormat);
                }
            }
            base.OnPaint(e);
        }
    }
}