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

using System;
using System.Reflection;
using System.Collections.Generic;

using Sulakore.Habbo.Web;
using Sulakore.Communication;

namespace Sulakore.Extensions
{
    public class ExtensionInfo
    {
        public string FileLocation { get; set; }
        public string Creator { get; }
        public string Identifier { get; }
        public string Description { get; }

        public HGameData GameData { get; }
        public Version Version { get; set; }
        public HTriggers Triggers { get; }

        public HHotel Hotel { get; }
        public IHConnection Connection { get; }

        public ExtensionInfo(Assembly extensionAssembly,
            HGameData gameData, HHotel hotel, IHConnection connection)
        {
            Hotel = hotel;
            GameData = gameData;
            Connection = connection;
            Triggers = new HTriggers(false);

            IList<CustomAttributeData> assemblyAttributes =
                extensionAssembly.GetCustomAttributesData();

            foreach (CustomAttributeData assemblyAttribute in assemblyAttributes)
            {
                object firstAttributeValue = assemblyAttribute.ConstructorArguments.Count > 0 ?
                    assemblyAttribute.ConstructorArguments[0].Value : null;

                switch (assemblyAttribute.AttributeType.Name)
                {
                    case "AssemblyTitleAttribute":
                    Identifier = (string)firstAttributeValue; break;

                    case "AssemblyCompanyAttribute":
                    {
                        Creator = (string)firstAttributeValue;
                        if (string.IsNullOrWhiteSpace(Creator))
                            Creator = "Unknown";
                        break;
                    }

                    case "AssemblyDescriptionAttribute":
                    Description = (string)firstAttributeValue; break;
                }
            }
        }
    }
}