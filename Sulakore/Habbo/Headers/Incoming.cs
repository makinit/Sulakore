/*
    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    This file is part of the Sulakore library.
    Copyright (C) 2015 ArachisH
    
    This code is licensed under the GNU General Public License.
    See License.txt in the project root for license information.
*/

using System.IO;
using System.Runtime.Serialization.Json;

namespace Sulakore.Habbo.Headers
{
    public class Incoming
    {
        private static readonly DataContractJsonSerializer _serializer =
            new DataContractJsonSerializer(typeof(Incoming));

        private static readonly Incoming _global = new Incoming();
        public static Incoming Global
        {
            get { return _global; }
        }

        public const ushort SERVER_DISCONNECT = 4000;

        public ushort RoomMapLoaded { get; set; }
        public ushort LocalHotelAlert { get; set; }
        public ushort GlobalHotelAlert { get; set; }

        public ushort EntityLoad { get; set; }
        public ushort FurnitureLoad { get; set; }

        public ushort PlayerUpdate { get; set; }
        public ushort PlayerUpdateStance { get; set; }

        public ushort PlayerDance { get; set; }
        public ushort PlayerGesture { get; set; }
        public ushort PlayerKickHost { get; set; }

        public ushort FurnitureDrop { get; set; }
        public ushort FurnitureMove { get; set; }

        public ushort PlayerSay { get; set; }
        public ushort PlayerShout { get; set; }
        public ushort PlayerWhisper { get; set; }

        public void Save(string path)
        {
            using (var fileStream = File.Open(path, FileMode.Create))
                _serializer.WriteObject(fileStream, this);
        }
        public static Incoming Load(string path)
        {
            using (var fileStream = File.Open(path, FileMode.Open))
                return (Incoming)_serializer.ReadObject(fileStream);
        }
    }
}