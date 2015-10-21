/*
    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    This file is part of the Sulakore library.
    Copyright (C) 2015 ArachisH
    
    This code is licensed under the GNU General Public License.
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