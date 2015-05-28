﻿/* Copyright

    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    .NET library for creating Habbo Hotel desktop applications.
    Copyright (C) 2015 Arachis

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    See License.txt in the project root for license information.
*/

namespace Sulakore.Habbo
{
    /// <summary>
    /// Specifies the different types of room models you can create in-game.
    /// </summary>
    public enum HModel
    {
        /// <summary>
        /// Represents a room with 104 tiles(8x13).
        /// </summary>
        A = 'a',
        /// <summary>
        /// Represents a room with 94 tiles((11x10) - (4x4)).
        /// </summary>
        B = 'b',
        /// <summary>
        /// Represents a room with 36 tiles(6x6).
        /// </summary>
        C = 'c',
        /// <summary>
        /// Represents a room with 84 tiles(6x14).
        /// </summary>
        D = 'd',
        /// <summary>
        /// Represents a room with 80 tiles(10x8).
        /// </summary>
        E = 'e',
        /// <summary>
        /// Represents a room with 84 tiles((10x10) - ((2x4)x2))
        /// </summary>
        F = 'f',
        /// <summary>
        /// Represents a room with 416 tiles(16x26).
        /// </summary>
        I = 'i',
        /// <summary>
        /// Represents a room with 380 tiles((20x22) - (10x6)).
        /// </summary>
        J = 'j',
        /// <summary>
        /// Represents a room with 448 tiles(((24x26) - (8x10)) - ((8x4)x3)).
        /// </summary>
        K = 'k',
        /// <summary>
        /// Represents a room with 352 tiles((20x20) - (4x12))
        /// </summary>
        L = 'l',
        /// <summary>
        /// Represents a room with 704 tiles((28x28) - (10x8))
        /// </summary>
        M = 'm',
        /// <summary>
        /// Represents a room with 368 tiles((20x20) - (8x4)).
        /// </summary>
        N = 'n',
        /// <summary>
        /// Represents a room with 35 tiles(5x7).
        /// </summary>
        S = 's'
    }
}