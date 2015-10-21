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
    /// Represents an in-game object that provides a unique identifier relative to the room.
    /// </summary>
    public interface IHEntity
    {
        /// <summary>
        /// Gets or sets the room index value of the <see cref="IHEntity"/>.
        /// </summary>
        int Index { get; }
    }
}