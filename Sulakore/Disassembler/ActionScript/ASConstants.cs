using System;
using System.Diagnostics;
using System.Collections.Generic;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Disassembler.ActionScript
{
    [DebuggerDisplay("Integers: {Integers.Count}, UIntegers: {UIntegers.Count}, Doubles: {Doubles.Count}, Strings: {Strings.Count}")]
    public class ASConstants : IABCChild
    {
        private readonly FlashReader _reader;

        public ABCFile ABC { get; }

        private readonly List<int> _integers;
        public IReadOnlyList<int> Integers => _integers;

        private readonly List<uint> _uintegers;
        public IReadOnlyList<uint> UIntegers => _uintegers;

        private readonly List<double> _doubles;
        public IReadOnlyList<double> Doubles => _doubles;

        private readonly List<string> _strings;
        public IReadOnlyList<string> Strings => _strings;

        private readonly List<ASNamespace> _namespaces;
        public IReadOnlyList<ASNamespace> Namespaces => _namespaces;

        private readonly List<ASNamespaceSet> _namespaceSets;
        public IReadOnlyList<ASNamespaceSet> NamespaceSets => _namespaceSets;

        private readonly List<ASMultiname> _multinames;
        public IReadOnlyList<ASMultiname> Multinames => _multinames;

        public ASConstants(ABCFile abc, FlashReader reader)
        {
            ABC = abc;

            _reader = reader;
            _integers = new List<int>();
            _uintegers = new List<uint>();
            _doubles = new List<double>();
            _strings = new List<string>();
            _namespaces = new List<ASNamespace>();
            _namespaceSets = new List<ASNamespaceSet>();
            _multinames = new List<ASMultiname>();
        }

        public int AddByte(byte value)
        {
            return AddInteger(value);
        }
        public int AddInteger(int value)
        {
            return AddValue(_integers, value);
        }
        public int AddUInteger(uint value)
        {
            return AddValue(_uintegers, value);
        }
        public int AddDouble(double value)
        {
            return AddValue(_doubles, value);
        }
        public int AddString(string value)
        {
            return AddValue(_strings, value);
        }
        protected virtual int AddValue<T>(List<T> valueList, T value)
        {
            int valueIndex = valueList.IndexOf(value);
            if (valueIndex == -1)
            {
                valueList.Add(value);
                valueIndex = (valueList.Count - 1);
            }
            return valueIndex;
        }

        public void SetByte(int index, byte value)
        {
            SetInteger(index, value);
        }
        public void SetInteger(int index, int value)
        {
            SetValue(index, value, _integers);
        }
        public void SetUInteger(int index, uint value)
        {
            SetValue(index, value, _uintegers);
        }
        public void SetDouble(int index, double value)
        {
            SetValue(index, value, _doubles);
        }
        public void SetString(int index, string value)
        {
            SetValue(index, value, _strings);
        }
        protected virtual void SetValue<T>(int index, T value, List<T> valueList)
        {
            valueList[index] = value;
        }

        public int IndexOfMultiname(string name)
        {
            for (int i = 1; i < _multinames.Count; i++)
                if (_multinames[i].ObjName == name) return i;

            return -1;
        }

        public void ReadConstants()
        {
            _integers.Capacity = _reader.Read7BitEncodedInt();
            if (_integers.Capacity > 0)
            {
                _integers.Add(0);
                for (int i = 1; i < _integers.Capacity; i++)
                    _integers.Add(_reader.Read7BitEncodedInt());
            }

            _uintegers.Capacity = _reader.Read7BitEncodedInt();
            if (_uintegers.Capacity > 0)
            {
                _uintegers.Add(0);
                for (int i = 1; i < _uintegers.Capacity; i++)
                    _uintegers.Add((uint)_reader.Read7BitEncodedInt());
            }

            _doubles.Capacity = _reader.Read7BitEncodedInt();
            if (_doubles.Capacity > 0)
            {
                _doubles.Add(double.NaN);
                for (int i = 1; i < _doubles.Capacity; i++)
                    _doubles.Add(_reader.ReadDouble());
            }

            _strings.Capacity = _reader.Read7BitEncodedInt();
            if (_strings.Capacity > 0)
            {
                _strings.Add(string.Empty);
                for (int i = 1; i < _strings.Capacity; i++)
                    _strings.Add(_reader.ReadString());
            }

            _namespaces.Capacity = _reader.Read7BitEncodedInt();
            if (_namespaces.Capacity > 0)
            {
                _namespaces.Add(null);
                for (int i = 1; i < _namespaces.Capacity; i++)
                    _namespaces.Add(new ASNamespace(ABC, _reader));
            }

            _namespaceSets.Capacity = _reader.Read7BitEncodedInt();
            if (_namespaceSets.Capacity > 0)
            {
                _namespaceSets.Add(null);
                for (int i = 1; i < _namespaceSets.Capacity; i++)
                    _namespaceSets.Add(new ASNamespaceSet(ABC, _reader));
            }

            _multinames.Capacity = _reader.Read7BitEncodedInt();
            if (_multinames.Capacity > 0)
            {
                _multinames.Add(null);
                for (int i = 1; i < _multinames.Capacity; i++)
                    _multinames.Add(new ASMultiname(ABC, _reader));
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

                case ConstantType.String: return _strings[index];
                case ConstantType.Double: return _doubles[index];
                case ConstantType.Integer: return _integers[index];
                case ConstantType.UInteger: return _uintegers[index];

                case ConstantType.Namespace:
                case ConstantType.PrivateNamespace:
                case ConstantType.PackageNamespace:
                case ConstantType.ExplicitNamespace:
                case ConstantType.ProtectedNamespace:
                case ConstantType.PackageInternalNamespace:
                case ConstantType.StaticProtectedNamespace: return _namespaces[index];

                default: throw new Exception("Invalid constant: " + type);
            }
        }

        public byte[] ToByteArray()
        {
            using (var asConstants = new FlashWriter())
            {
                asConstants.Write7BitEncodedInt(_integers.Count);
                for (int i = 1; i < _integers.Count; i++)
                    asConstants.Write7BitEncodedInt(_integers[i]);

                asConstants.Write7BitEncodedInt(_uintegers.Count);
                for (int i = 1; i < _uintegers.Count; i++)
                    asConstants.Write7BitEncodedInt((int)_uintegers[i]);

                asConstants.Write7BitEncodedInt(_doubles.Count);
                for (int i = 1; i < _doubles.Count; i++)
                    asConstants.Write(_doubles[i]);

                asConstants.Write7BitEncodedInt(_strings.Count);
                for (int i = 1; i < _strings.Count; i++)
                    asConstants.Write(_strings[i]);

                asConstants.Write7BitEncodedInt(_namespaces.Count);
                for (int i = 1; i < _namespaces.Count; i++)
                    asConstants.Write(_namespaces[i].ToByteArray());

                asConstants.Write7BitEncodedInt(_namespaceSets.Count);
                for (int i = 1; i < _namespaceSets.Count; i++)
                    asConstants.Write(_namespaceSets[i].ToByteArray());

                asConstants.Write7BitEncodedInt(_multinames.Count);
                for (int i = 1; i < _multinames.Count; i++)
                    asConstants.Write(_multinames[i].ToByteArray());

                return asConstants.ToArray();
            }
        }
    }
}