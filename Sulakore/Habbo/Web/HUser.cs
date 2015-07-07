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
    public class HUser
    {
        private static readonly DataContractJsonSerializer _deserializer =
            new DataContractJsonSerializer(typeof(HUser));

        [DataMember(Name = "uniqueId")]
        public string UniqueId { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "figureString")]
        public string FigureId { get; set; }

        [DataMember(Name = "selectedBadges")]
        public IList<HBadge> SelectedBadges { get; set; }

        [DataMember(Name = "motto")]
        public string Motto { get; set; }

        [DataMember(Name = "memberSince")]
        public string MemberSince { get; set; }

        [DataMember(Name = "profileVisible")]
        public bool IsProfileVisible { get; set; }

        [DataMember(Name = "lastWebAccess")]
        public string LastWebAccess { get; set; }

        [DataMember(Name = "sessionLogId", EmitDefaultValue = false)]
        public long SessionLogId { get; set; }

        [DataMember(Name = "loginLogId", EmitDefaultValue = false)]
        public long LoginLodId { get; set; }

        [DataMember(Name = "email", EmitDefaultValue = false)]
        public string Email { get; set; }

        [DataMember(Name = "identityId", EmitDefaultValue = false)]
        public int IdentityId { get; set; }

        [DataMember(Name = "emailVerified", EmitDefaultValue = false)]
        public bool IsEmailVerified { get; set; }

        [DataMember(Name = "trusted", EmitDefaultValue = false)]
        public bool IsTrusted { get; set; }

        [DataMember(Name = "accountId", EmitDefaultValue = false)]
        public int AccountId { get; set; }

        [DataMember(Name = "country", EmitDefaultValue = false)]
        public string Country { get; set; }

        [DataMember(Name = "traits", EmitDefaultValue = false)]
        public string Traits { get; set; }

        [DataMember(Name = "partner", EmitDefaultValue = false)]
        public string Partner { get; set; }

        public static HUser Create(string userJson)
        {
            byte[] rawJson = Encoding.UTF8.GetBytes(userJson);
            using (var jsonStream = new MemoryStream(rawJson))
                return (HUser)_deserializer.ReadObject(jsonStream);
        }
    }
}