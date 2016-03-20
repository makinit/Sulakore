using System;

namespace Sulakore.Modules
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ModuleAttribute : Attribute
    {
        public string Name { get; }

        public ModuleAttribute(string name)
        {
            Name = name;
        }
    }
}