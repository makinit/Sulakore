/*
    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    This file is part of the Sulakore library.
    Copyright (C) 2015 ArachisH
    
    This code is licensed under the GNU General Public License.
    See License.txt in the project root for license information.
*/

using Sulakore.Habbo;

using System.Windows.Forms;

namespace Sulakore.Components
{
    public class TileEventArgs : MouseEventArgs
    {
        public HPoint Tile { get; }

        public TileEventArgs(HPoint tile,
            MouseButtons button, int clicks, int x, int y, int delta)
            : base(button, clicks, x, y, delta)
        {
            Tile = tile;
        }
    }
}