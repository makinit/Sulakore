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