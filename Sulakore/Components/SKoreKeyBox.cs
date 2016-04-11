using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sulakore.Components
{
    [DesignerCategory("Code")]
    public class SKoreKeyBox : TextBox
    {
        private SKoreHotkey.KeyCombination _value;
        public SKoreHotkey.KeyCombination Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                this.Text = value?.ToString() ?? "";
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            e.SuppressKeyPress = true;

            this.Value = new SKoreHotkey.KeyCombination(e.Modifiers, e.KeyCode);
        }
    }
}
