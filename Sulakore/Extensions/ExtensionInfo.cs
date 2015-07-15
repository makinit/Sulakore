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

using Sulakore.Communication;

namespace Sulakore.Extensions
{
    public class ExtensionInfo
    {
        public string FileLocation { get; set; }
        public string Creator { get; private set; }
        public string Identifier { get; private set; }
        public string Description { get; private set; }

        public Version Version { get; set; }
        public HTriggers Triggers { get; private set; }

        public HHotel Hotel { get; private set; }
        public IHConnection Connection { get; private set; }

        public ExtensionInfo(Assembly extensionAssembly, HHotel hotel, IHConnection connection)
        {
            Hotel = hotel;
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
                    Creator = (string)firstAttributeValue; break;

                    case "AssemblyDescriptionAttribute":
                    Description = (string)firstAttributeValue; break;
                }
            }
        }
    }
}