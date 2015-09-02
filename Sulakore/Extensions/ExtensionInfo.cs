/* Copyright

    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    .NET library for creating Habbo Hotel related desktop applications.
    Copyright (C) 2015 ArachisH

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

using Sulakore.Habbo.Web;
using Sulakore.Communication;

namespace Sulakore.Extensions
{
    public class ExtensionInfo
    {
        public string Hash { get; }
        public HHotel Hotel { get; }
        public HGameData GameData { get; }
        public string FileLocation { get; }
        public IHConnection Connection { get; }

        public ExtensionInfo(string fileLocation, string hash,
            HGameData gameData, HHotel hotel, IHConnection connection)
        {
            Hash = hash;
            Hotel = hotel;
            GameData = gameData;
            Connection = connection;
            FileLocation = fileLocation;
        }
    }
}