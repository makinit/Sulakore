﻿/*
    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    This file is part of the Sulakore library.
    Copyright (C) 2015 ArachisH
    
    This code is licensed under the GNU General Public License.
    See License.txt in the project root for license information.
*/

namespace Sulakore.Habbo
{
    /// <summary>
    /// Specifies a set of gestures a player can perform in-game.
    /// </summary>
    public enum HGesture
    {
        /// <summary>
        /// Represents a player without any gesture.
        /// </summary>
        None = 0,
        /// <summary>
        /// Represents a player waving.
        /// </summary>
        Wave = 1,
        /// <summary>
        /// Represents a player blowing a kiss. (HC Only)
        /// </summary>
        BlowKiss = 2,
        /// <summary>
        /// Represents a player laughing. (HC Only)
        /// </summary>
        Laugh = 3,
        /// <summary>
        /// Represents a player sleeping.
        /// </summary>
        Idle = 5,
        /// <summary>
        /// Represents a player hopping once. (HC Only)
        /// </summary>
        PogoHop = 6,
        /// <summary>
        /// Represents a player raising a thumbs up.
        /// </summary>
        ThumbsUp = 7
    }
}