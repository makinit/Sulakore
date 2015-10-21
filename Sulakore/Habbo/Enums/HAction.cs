/*
    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    This file is part of the Sulakore library.
    Copyright (C) 2015 ArachisH
    
    This code is licensed under the GNU General Public License.
    See License.txt in the project root for license information.
*/

namespace Sulakore.Habbo
{
    /// <summary>
    /// Specifies a set of actions a player can perform in-game
    /// </summary>
    public enum HAction
    {
        /// <summary>
        /// Represents a player not performing any actions.
        /// </summary>
        None = 0,
        /// <summary>
        /// Represents a moving player.
        /// </summary>
        Move = 1,
        /// <summary>
        /// Represents a player that has sat down.
        /// </summary>
        Sit = 2,
        /// <summary>
        /// Represents a player that has laid down.
        /// </summary>
        Lay = 3,
        /// <summary>
        /// Represents a player holding up a <see cref="HSign"/>.
        /// </summary>
        Sign = 4
    }
}