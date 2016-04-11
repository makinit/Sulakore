using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sulakore.Components
{
    [DesignerCategory("Code")]
    public class SKoreHotkey : Component
    {
        private const int MOD_ALT = 0x1;
        private const int MOD_CONTROL = 0x2;
        private const int MOD_SHIFT = 0x4;

        private const int WM_HOTKEY = 0x312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private KeyCombination _keys;
        public KeyCombination Keys
        {
            get
            {
                return _keys;
            }
            set
            {
                if (_keys != null)
                    UnregisterHotKey((IntPtr)_control.Handle, _control.GetHashCode());

                _keys = value;

                if (_keys != null)
                    RegisterHotKey((IntPtr)_control.Handle, _control.GetHashCode(),
                        (uint)((_keys.Alt ? MOD_ALT : 0x0) | (_keys.Control ? MOD_CONTROL : 0x0) | (_keys.Shift ? MOD_SHIFT : 0x0)),
                        (uint)_keys.KeyCode);
            }
        }

        private Control _control;

        public SKoreHotkey(KeyCombination keys)
        {
            _control = new HotkeyControl(Callback);
            Keys = keys;
        }

        public event EventHandler<EventArgs> Press;

        private void Callback()
        {
            Press?.Invoke(this, new EventArgs());
        }

        protected override void Dispose(bool disposing)
        {
            Keys = null;
            _control.Dispose();
            base.Dispose(disposing);
        }

        private class HotkeyControl : Control
        {
            private Action _callback;

            public HotkeyControl(Action callback)
            {
                _callback = callback;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY && (int)m.WParam == this.GetHashCode())
                {
                    
                    Invoke(_callback);
                }
                base.WndProc(ref m);
            }
        }

        public class KeyCombination
        {
            public bool Shift { get; private set; }
            public bool Control { get; private set; }
            public bool Alt { get; private set; }
            public Keys KeyCode { get; private set; }

            public KeyCombination(Keys modifiers, Keys code)
            {
                this.Shift = modifiers.HasFlag(System.Windows.Forms.Keys.Shift);
                this.Control = modifiers.HasFlag(System.Windows.Forms.Keys.Control);
                this.Alt = modifiers.HasFlag(System.Windows.Forms.Keys.Alt);
                this.KeyCode = code;
            }

            public override string ToString()
            {
                string res = "";

                if (Control)
                    res += "Ctrl+";
                if (Shift)
                    res += "Shift+";
                if (Alt)
                    res += "Alt+";

                res += KeyCode.ToString();

                return res;
            }
        }
    }
}
