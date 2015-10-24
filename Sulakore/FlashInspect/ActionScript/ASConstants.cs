using System;
using System.Collections.Generic;

using FlashInspect.IO;
using FlashInspect.ActionScript.Constants;

namespace FlashInspect.ActionScript
{
    public class ASConstants
    {
        private readonly ABCFile _abc;
        private readonly FlashReader _reader;

        public List<int> Integers { get; }
        public List<uint> UIntegers { get; }
        public List<double> Doubles { get; }
        public List<string> Strings { get; }
        public List<ASNamespace> Namespaces { get; }
        public List<ASNamespaceSet> NamespaceSets { get; }
        public List<ASMultiname> Multinames { get; }

        public ASConstants(ABCFile abc, FlashReader reader)
        {
            _abc = abc;
            _reader = reader;

            Integers = new List<int>();
            UIntegers = new List<uint>();
            Doubles = new List<double>();
            Strings = new List<string>();
            Namespaces = new List<ASNamespace>();
            NamespaceSets = new List<ASNamespaceSet>();
            Multinames = new List<ASMultiname>();
        }

        public void ReadConstants()
        {
            Integers.Capacity = _reader.Read7BitEncodedInt();
            if (Integers.Capacity > 0)
            {
                Integers.Add(0);
                for (int i = 1; i < Integers.Capacity; i++)
                    Integers.Add(_reader.Read7BitEncodedInt());
            }

            UIntegers.Capacity = _reader.Read7BitEncodedInt();
            if (UIntegers.Capacity > 0)
            {
                UIntegers.Add(0);
                for (int i = 1; i < UIntegers.Capacity; i++)
                    UIntegers.Add((uint)_reader.Read7BitEncodedInt());
            }

            Doubles.Capacity = _reader.Read7BitEncodedInt();
            if (Doubles.Capacity > 0)
            {
                Doubles.Add(double.NaN);
                for (int i = 1; i < Doubles.Capacity; i++)
                    Doubles.Add(_reader.ReadDouble());
            }

            Strings.Capacity = _reader.Read7BitEncodedInt();
            if (Strings.Capacity > 0)
            {
                Strings.Add(string.Empty);
                for (int i = 1; i < Strings.Capacity; i++)
                    Strings.Add(_reader.ReadString());
            }

            Namespaces.Capacity = _reader.Read7BitEncodedInt();
            if (Namespaces.Capacity > 0)
            {
                Namespaces.Add(null);
                for (int i = 1; i < Namespaces.Capacity; i++)
                    Namespaces.Add(new ASNamespace(this, _reader));
            }

            NamespaceSets.Capacity = _reader.Read7BitEncodedInt();
            if (NamespaceSets.Capacity > 0)
            {
                NamespaceSets.Add(null);
                for (int i = 1; i < NamespaceSets.Capacity; i++)
                    NamespaceSets.Add(new ASNamespaceSet(this, _reader));
            }

            Multinames.Capacity = _reader.Read7BitEncodedInt();
            if (Multinames.Capacity > 0)
            {
                Multinames.Add(null);
                for (int i = 1; i < Multinames.Capacity; i++)
                    Multinames.Add(new ASMultiname(this, _reader));
            }
        }
        public object GetValue(ConstantType type, int index)
        {
            switch (type)
            {
                case ConstantType.Null:
                case ConstantType.Undefined: return null;

                case ConstantType.True: return true;
                case ConstantType.False: return false;

                case ConstantType.String: return Strings[index];
                case ConstantType.Double: return Doubles[index];
                case ConstantType.Integer: return Integers[index];
                case ConstantType.UInteger: return UIntegers[index];

                case ConstantType.Namespace:
                case ConstantType.PrivateNamespace:
                case ConstantType.PackageNamespace:
                case ConstantType.ExplicitNamespace:
                case ConstantType.ProtectedNamespace:
                case ConstantType.PackageInternalNamespace:
                case ConstantType.StaticProtectedNamespace: return Namespaces[index];

                default: throw new Exception("Invalid constant: " + type);
            }
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(Integers.Count);
                for (int i = 1; i < Integers.Count; i++)
                    abc.Write7BitEncodedInt(Integers[i]);

                abc.Write7BitEncodedInt(UIntegers.Count);
                for (int i = 1; i < UIntegers.Count; i++)
                    abc.Write7BitEncodedInt((int)UIntegers[i]);

                abc.Write7BitEncodedInt(Doubles.Count);
                for (int i = 1; i < Doubles.Count; i++)
                    abc.Write(Doubles[i]);

                abc.Write7BitEncodedInt(Strings.Count);
                for (int i = 1; i < Strings.Count; i++)
                    abc.Write(Strings[i]);

                abc.Write7BitEncodedInt(Namespaces.Count);
                for (int i = 1; i < Namespaces.Count; i++)
                    abc.Write(Namespaces[i].ToArray());

                abc.Write7BitEncodedInt(NamespaceSets.Count);
                for (int i = 1; i < NamespaceSets.Count; i++)
                    abc.Write(NamespaceSets[i].ToArray());

                abc.Write7BitEncodedInt(Multinames.Count);
                for (int i = 1; i < Multinames.Count; i++)
                    abc.Write(Multinames[i].ToArray());

                return abc.ToArray();
            }
        }
    }
}