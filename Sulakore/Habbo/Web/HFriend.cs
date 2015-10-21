/*
    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    This file is part of the Sulakore library.
    Copyright (C) 2015 ArachisH
    
    This code is licensed under the GNU General Public License.
    See License.txt in the project root for license information.
*/

using System.Runtime.Serialization;

namespace Sulakore.Habbo.Web
{
    [DataContract]
    public class HFriend
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "motto")]
        public string Motto { get; set; }

        [DataMember(Name = "uniqueId")]
        public string UniqueId { get; set; }

        [DataMember(Name = "figureString")]
        public string FigureId { get; set; }
    }
}