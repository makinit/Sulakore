using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sulakore.Components
{
    public static class Draw
    {
        public const int MStateNone = 0, MStateOver = 1, MStateDown = 2;

        public static Image Scale(this Image image, int height, int width)
        {
            if (image == null || height <= 0 || width <= 0) return null;

            var scaledR = new Rectangle(0, 0,
                (image.Width * height) / (image.Height), (image.Height * width) / (image.Width));

            var scaledB = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(scaledB))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                if (scaledR.Width > width)
                {
                    scaledR.X = (scaledB.Width - width) / 2;
                    scaledR.Y = (scaledB.Height - scaledR.Height) / 2;
                    graphics.DrawImage(image, scaledR.X, scaledR.Y, width, scaledR.Height);
                }
                else
                {
                    scaledR.X = (scaledB.Width / 2) - (scaledR.Width / 2);
                    scaledR.Y = (scaledB.Height / 2) - (height / 2);
                    graphics.DrawImage(image, scaledR.X, scaledR.Y, scaledR.Width, height);
                }
            }
            return scaledB;
        }

        public static GraphicsPath Round(this Rectangle rectangle, int curve)
        {
            var graphicsPath = new GraphicsPath();
            if (curve <= 0)
            {
                graphicsPath.AddRectangle(rectangle);
                return graphicsPath;
            }

            int arw = curve * 2;
            graphicsPath.AddArc(new Rectangle(rectangle.X, rectangle.Y, arw, arw), -180, 90);
            graphicsPath.AddArc(new Rectangle(rectangle.Width - arw + rectangle.X, rectangle.Y, arw, arw), -90, 90);
            graphicsPath.AddArc(new Rectangle(rectangle.Width - arw + rectangle.X, rectangle.Height - arw + rectangle.Y, arw, arw), 0, 90);
            graphicsPath.AddArc(new Rectangle(rectangle.X, rectangle.Height - arw + rectangle.Y, arw, arw), 90, 90);
            graphicsPath.AddLine(new Point(rectangle.X, rectangle.Height - arw + rectangle.Y), new Point(rectangle.X, curve + rectangle.Y));
            return graphicsPath;
        }
        public static GraphicsPath Triangle(this Rectangle rectangle, float angle = 90)
        {
            using (var matrix = new Matrix())
            {
                matrix.RotateAt(angle, new Point(rectangle.X + (rectangle.Width / 2), rectangle.Y + (rectangle.Height / 2)));
                var graphicsPath = new GraphicsPath();
                graphicsPath.AddLine(new Point(rectangle.X, rectangle.Y + (rectangle.Height / 2)), new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height));
                graphicsPath.AddLine(new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height), new Point(rectangle.X + rectangle.Width, rectangle.Y));
                graphicsPath.CloseAllFigures();
                graphicsPath.Transform(matrix);
                return graphicsPath;
            }
        }

        public static LinearGradientBrush CreateGradient(this Color color1, Color color2, int x, int y, int width, int height, float angle = 90)
        {
            return color1.CreateGradient(color2, new Rectangle(x, y, width, height), angle);
        }
        public static LinearGradientBrush CreateGradient(this Color color1, Color color2, Rectangle rectangle, float angle = 90)
        {
            return new LinearGradientBrush(rectangle, color1, color2, angle);
        }

        public static void DrawLine(this Graphics graphics, Color color1, int x1, int y1, int x2, int y2)
        {
            graphics.DrawLine(color1, new Point(x1, y1), new Point(x2, y2));
        }
        public static void DrawLine(this Graphics graphics, Color color1, Point point, int x, int y)
        {
            graphics.DrawLine(color1, point, new Point(x, y));
        }
        public static void DrawLine(this Graphics graphics, Color color1, int x, int y, Point point)
        {
            graphics.DrawLine(color1, new Point(x, y), point);
        }
        public static void DrawLine(this Graphics graphics, Color color1, Point pt1, Point pt2)
        {
            using (var pen = new Pen(color1))
                graphics.DrawLine(pen, pt1, pt2);
        }

        public static void DrawString(this Graphics graphics, string text, Font font, Color color, int x, int y, int width, int height)
        {
            graphics.DrawString(text, font, color, new Rectangle(x, y, width, height));
        }
        public static void DrawString(this Graphics graphics, string text, Font font, Color color, Rectangle rectangle)
        {
            using (var solidBrush = new SolidBrush(color))
                graphics.DrawString(text, font, solidBrush, rectangle);
        }
        public static void DrawString(this Graphics graphics, string text, Font font, Color color, int x, int y, int width, int height, StringAlignment stringAlignment)
        {
            graphics.DrawString(text, font, color, new Rectangle(x, y, width, height), stringAlignment);
        }
        public static void DrawString(this Graphics graphics, string text, Font font, Color color, Rectangle rectangle, StringAlignment stringAlignment)
        {
            using (var solidBrush = new SolidBrush(color))
            using (var stringFormat = new StringFormat { Alignment = stringAlignment, LineAlignment = stringAlignment })
                graphics.DrawString(text, font, solidBrush, rectangle, stringFormat);
        }

        public static void DrawString(this Graphics graphics, string text, Font font, Color color, int x, int y, int width, int height, StringAlignment alignment, StringAlignment lineAlignment)
        {
            graphics.DrawString(text, font, color, new Rectangle(x, y, width, height), alignment, lineAlignment);
        }
        public static void DrawString(this Graphics graphics, string text, Font font, Color color, Rectangle rectangle, StringAlignment alignment, StringAlignment lineAlignment)
        {
            using (var solidBrush = new SolidBrush(color))
            using (var stringFormat = new StringFormat { Alignment = alignment, LineAlignment = lineAlignment })
                graphics.DrawString(text, font, solidBrush, rectangle, stringFormat);
        }

        public static void FillGradient(this Graphics graphics, Color color1, Color color2, int x, int y, int width, int height, float angle = 90)
        {
            graphics.FillGradient(color1, color2, new Rectangle(x, y, width, height), angle);
        }
        public static void FillGradient(this Graphics graphics, Color color1, Color color2, Rectangle rectangle, float angle = 90)
        {
            using (var linearGradientBrush = new LinearGradientBrush(rectangle, color1, color2, angle))
                graphics.FillRectangle(linearGradientBrush, rectangle);
        }

        public static void FillRoundGradient(this Graphics graphics, Color color1, Color color2, int x, int y, int width, int height, float angle, int curve)
        {
            graphics.FillRoundGradient(color1, color2, new Rectangle(x, y, width, height), angle, curve);
        }
        public static void FillRoundGradient(this Graphics graphics, Color color1, Color color2, Rectangle rectangle, float angle, int curve)
        {
            using (var linearGradientBrush = new LinearGradientBrush(rectangle, color1, color2, angle))
            using (GraphicsPath graphicsPath = rectangle.Round(curve))
                graphics.FillPath(linearGradientBrush, graphicsPath);
        }

        public static void FillRound(this Graphics graphics, Color color, int x, int y, int width, int height, int curve)
        {
            graphics.FillRound(color, new Rectangle(x, y, width, height), curve);
        }
        public static void FillRound(this Graphics graphics, Color color, Rectangle rectangle, int curve)
        {
            using (var solidBrush = new SolidBrush(color))
            using (GraphicsPath graphicsPath = rectangle.Round(curve))
                graphics.FillPath(solidBrush, graphicsPath);
        }

        public static void DrawRound(this Graphics graphics, Color color, int x, int y, int width, int height, int curve)
        {
            graphics.DrawRound(color, new Rectangle(x, y, width, height), curve);
        }
        public static void DrawRound(this Graphics graphics, Color color, Rectangle rectangle, int curve)
        {
            using (var pen = new Pen(color))
            using (GraphicsPath graphicsPath = rectangle.Round(curve))
                graphics.DrawPath(pen, graphicsPath);
        }

        public static void FillTriangle(this Graphics graphics, Color color, int x, int y, int width, int height, float angle = 90)
        {
            graphics.FillTriangle(color, new Rectangle(x, y, width, height), angle);
        }
        public static void FillTriangle(this Graphics graphics, Color color, Rectangle rectangle, float angle = 90)
        {
            using (GraphicsPath graphicsPath = rectangle.Triangle(angle))
            using (var solidBrush = new SolidBrush(color))
                graphics.FillPath(solidBrush, graphicsPath);
        }

        public static void FillRectangle(this Graphics graphics, Color color, int x, int y, int width, int height)
        {
            graphics.FillRectangle(color, new Rectangle(x, y, width, height));
        }
        public static void FillRectangle(this Graphics graphics, Color color, Rectangle rectangle)
        {
            using (var solidBrush = new SolidBrush(color))
                graphics.FillRectangle(solidBrush, rectangle);
        }

        public static void DrawRectangle(this Graphics graphics, Color color, int x, int y, int width, int height)
        {
            graphics.DrawRectangle(color, new Rectangle(x, y, width, height));
        }
        public static void DrawRectangle(this Graphics graphics, Color color, Rectangle rectangle)
        {
            using (var pen = new Pen(color))
                graphics.DrawRectangle(pen, rectangle);
        }

        public static void DrawPixel(this Graphics graphics, Color color, int x, int y)
        {
            graphics.DrawPixel(color, new Point(x, y));
        }
        public static void DrawPixel(this Graphics graphics, Color color, Point point)
        {
            using (var solidBrush = new SolidBrush(color))
                graphics.FillRectangle(solidBrush, point.X, point.Y, 1, 1);
        }

        public static void DrawBorders(this Graphics graphics, Color color, int x, int y, int width, int height, int offset = 0)
        {
            graphics.DrawBorders(color, new Rectangle(x, y, width, height), offset);
        }
        public static void DrawBorders(this Graphics graphics, Color color, Rectangle rectangle, int offset = 0)
        {
            using (var pen = new Pen(color))
                graphics.DrawRectangle(pen, rectangle.X + offset, rectangle.Y + offset, (rectangle.Width - (offset * 2)) - 1, (rectangle.Height - (offset * 2) - 1));
        }
    }
}