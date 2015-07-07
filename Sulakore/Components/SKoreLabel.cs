using System;
using System.Timers;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sulakore.Components
{
    [DesignerCategory("Code")]
    public class SKoreLabel : Label
    {
        private string _format, _frames;
        private int _currentAnimationReach = 1;
        private readonly System.Timers.Timer _animateTimer;

        private int _borderWidth = 2;
        [DefaultValue(2)]
        public int BorderWidth
        {
            get { return _borderWidth; }
            set { _borderWidth = value; Invalidate(); }
        }

        private bool _displayBoundary;
        [DefaultValue(false)]
        public bool DisplayBoundary
        {
            get { return _displayBoundary; }
            set { _displayBoundary = value; Invalidate(); }
        }

        private Color _skin = Color.SteelBlue;
        [DefaultValue(typeof(Color), "SteelBlue")]
        public Color Skin
        {
            get { return _skin; }
            set { _skin = value; Invalidate(); }
        }

        private int _animationInterval = 250;
        [DefaultValue(250)]
        public int AnimationInterval
        {
            get { return _animationInterval; }
            set
            {
                _animationInterval = value;

                if (!DesignMode)
                    _animateTimer.Interval = value;
            }
        }

        public SKoreLabel()
        {
            SetStyle((ControlStyles)2050, true);
            DoubleBuffered = true;

            _animateTimer = new System.Timers.Timer(_animationInterval);
            _animateTimer.SynchronizingObject = this;
            _animateTimer.Elapsed += DoAnimate;
        }

        public void EndAnimation(string text)
        {
            _currentAnimationReach = 1;
            _animateTimer.Stop();

            if (!string.IsNullOrEmpty(text))
                FindForm().Invoke(new MethodInvoker(() => Text = text));
        }
        public void BeginAnimation(string format, string frames)
        {
            EndAnimation(string.Empty);

            if (string.IsNullOrWhiteSpace(format))
                throw new Exception("Invalid string format.");

            if (string.IsNullOrEmpty(frames) || frames.Length < 2)
                throw new Exception("frames must at least be two chars long.");

            _format = format;
            _frames = frames;

            FindForm().Invoke(new MethodInvoker(() => Text = string.Format(format, frames)));
            _animateTimer.Start();
        }
        private void DoAnimate(object sender, ElapsedEventArgs e)
        {
            if (!_animateTimer.Enabled) return;

            Text = string.Format(_format,
                _frames.Substring(0, _currentAnimationReach++));

            if (_currentAnimationReach > _frames.Length)
                _currentAnimationReach = 1;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            if (DisplayBoundary)
            {
                using (var solidBrush = new SolidBrush(Skin))
                {
                    e.Graphics.FillRectangle(solidBrush, 0, 0, BorderWidth, Height);
                    e.Graphics.FillRectangle(solidBrush, Width - BorderWidth, 0, BorderWidth, Height);
                }
            }

            base.OnPaint(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _animateTimer.Dispose();

            base.Dispose(disposing);
        }
    }
}