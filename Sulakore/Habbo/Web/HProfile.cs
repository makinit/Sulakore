﻿/*
    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    This file is part of the Sulakore library.
    Copyright (C) 2015 ArachisH
    
    This code is licensed under the GNU General Public License.
    See License.txt in the project root for license information.
*/

using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Sulakore.Habbo.Web
{
    [DataContract]
    public class HProfile
    {
        private static readonly DataContractJsonSerializer _deserializer =
            new DataContractJsonSerializer(typeof(HProfile));

        [DataMember(Name = "user")]
        public HUser User { get; set; }

        [DataMember(Name = "friends")]
        public IList<HFriend> Friends { get; set; }

        [DataMember(Name = "groups")]
        public IList<HGroup> Groups { get; set; }

        [DataMember(Name = "rooms")]
        public IList<HRoom> Rooms { get; set; }

        [DataMember(Name = "badges")]
        public IList<HBadge> Badges { get; set; }

        public static HProfile Create(string profileJson)
        {
            byte[] rawJson = Encoding.UTF8.GetBytes(profileJson);
            using (var jsonStream = new MemoryStream(rawJson))
                return (HProfile)_deserializer.ReadObject(jsonStream);
        }
    }
}