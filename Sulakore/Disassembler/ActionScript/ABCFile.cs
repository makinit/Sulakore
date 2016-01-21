using System.Diagnostics;
using System.Collections.Generic;

using Sulakore.Disassembler.IO;

namespace Sulakore.Disassembler.ActionScript
{
    [DebuggerDisplay("Classes/Instances: {Classes.Count}, Methods: {Methods.Count}, Version: {MajorVersion}.{MinorVersion}")]
    public class ABCFile
    {
        private readonly FlashReader _reader;

        public ushort MinorVersion { get; }
        public ushort MajorVersion { get; }

        public ASConstants Constants { get; }
        public List<ASClass> Classes { get; }
        public List<ASMethod> Methods { get; }
        public List<ASScript> Scripts { get; }
        public List<ASMetadata> Metadata { get; }
        public List<ASInstance> Instances { get; }
        public List<ASMethodBody> MethodBodies { get; }

        public ABCFile(byte[] data)
        {
            _reader = new FlashReader(data);

            MinorVersion = _reader.ReadUInt16();
            MajorVersion = _reader.ReadUInt16();

            Constants = new ASConstants(this, _reader);
            Constants.ReadConstants();

            Methods = new List<ASMethod>(_reader.Read7BitEncodedInt());
            for (int i = 0; i < Methods.Capacity; i++)
                Methods.Add(new ASMethod(this, _reader));

            Metadata = new List<ASMetadata>(_reader.Read7BitEncodedInt());
            for (int i = 0; i < Metadata.Capacity; i++)
                Metadata.Add(new ASMetadata(this, _reader));

            Instances = new List<ASInstance>(_reader.Read7BitEncodedInt());
            for (int i = 0; i < Instances.Capacity; i++)
                Instances.Add(new ASInstance(this, _reader));

            Classes = new List<ASClass>(Instances.Capacity);
            for (int i = 0; i < Classes.Capacity; i++)
            {
                Classes.Add(new ASClass(this, _reader));
                Classes[i].Instance = Instances[i];
            }

            Scripts = new List<ASScript>(_reader.Read7BitEncodedInt());
            for (int i = 0; i < Scripts.Capacity; i++)
                Scripts.Add(new ASScript(this, _reader));

            MethodBodies = new List<ASMethodBody>(_reader.Read7BitEncodedInt());
            for (int i = 0; i < MethodBodies.Capacity; i++)
                MethodBodies.Add(new ASMethodBody(this, _reader));
        }

        public ASClass FindClassByName(string className)
        {
            int classIndex = FindClassInstanceIndex(className);
            return classIndex < 0 ? null : Classes[classIndex];
        }
        public ASInstance FindInstanceByName(string instanceName)
        {
            int instanceIndex = FindClassInstanceIndex(instanceName);
            return instanceIndex < 0 ? null : Instances[instanceIndex];
        }

        protected int FindClassInstanceIndex(string name)
        {
            if (string.IsNullOrEmpty(name)) return -1;
            for (int i = 0; i < Instances.Count; i++)
            {
                if (Instances[i].Type.ObjName == name)
                    return i;
            }
            return -1;
        }

        public byte[] ToByteArray()
        {
            using (var abcFile = new FlashWriter())
            {
                abcFile.Write(MinorVersion);
                abcFile.Write(MajorVersion);
                abcFile.Write(Constants.ToByteArray());

                abcFile.Write7BitEncodedInt(Methods.Count);
                foreach (ASMethod methodInfo in Methods)
                    abcFile.Write(methodInfo.ToByteArray());

                abcFile.Write7BitEncodedInt(Metadata.Count);
                foreach (ASMetadata metadataInfo in Metadata)
                    abcFile.Write(metadataInfo.ToByteArray());

                abcFile.Write7BitEncodedInt(Instances.Count);
                foreach (ASInstance instanceInfo in Instances)
                    abcFile.Write(instanceInfo.ToByteArray());

                foreach (ASClass classInfo in Classes)
                    abcFile.Write(classInfo.ToByteArray());

                abcFile.Write7BitEncodedInt(Scripts.Count);
                foreach (ASScript scriptInfo in Scripts)
                    abcFile.Write(scriptInfo.ToByteArray());

                abcFile.Write7BitEncodedInt(MethodBodies.Count);
                foreach (ASMethodBody methodBodyInfo in MethodBodies)
                    abcFile.Write(methodBodyInfo.ToByteArray());

                return abcFile.ToArray();
            }
        }
    }
}