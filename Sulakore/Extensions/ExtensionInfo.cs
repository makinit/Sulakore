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