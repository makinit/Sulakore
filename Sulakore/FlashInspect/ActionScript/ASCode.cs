using System.Collections.Generic;

using FlashInspect.IO;

namespace FlashInspect.ActionScript
{
    public class ASCode : List<byte>
    {
        private readonly ABCFile _abc;
        private readonly ASMethodBody _body;

        public ASCode(ABCFile abc, ASMethodBody body)
        {
            _abc = abc;
            _body = body;
        }
        public ASCode(ABCFile abc, ASMethodBody body, int capacity) :
            base(capacity)
        {
            _abc = abc;
            _body = body;
        }
        public ASCode(ABCFile abc, ASMethodBody body, IEnumerable<byte> collection) :
            base(collection)
        {
            _abc = abc;
            _body = body;
        }

        public void AddPushString(string value)
        {
            InsertPushString(Count, value);
        }
        public void InsertPushString(int index, string value)
        {
            if (!_abc.Constants.Strings.Contains(value))
                _abc.Constants.Strings.Add(value);

            int valueIndex = _abc.Constants
                .Strings.IndexOf(value);

            InsertInstruction(index,
                OPCode.PushString, To7BitEncodedInt(valueIndex));
        }

        public void AddInstruction(OPCode operation, params byte[] arguments)
        {
            InsertInstruction(Count, operation, arguments);
        }
        public void InsertInstruction(int index, OPCode operation, params byte[] arguments)
        {
            var buffer = new byte[1 + arguments.Length];
            buffer[0] = (byte)operation;

            for (int i = 1; i < buffer.Length; i++)
                buffer[i] = arguments[i - 1];

            InsertRange(index, buffer);
        }

        private int Read7BitEncodedInt(ref int index)
        {
            using (var code = new FlashReader(ToArray()))
            {
                code.Position = index;
                int result = code.Read7BitEncodedInt();

                index = code.Position;
                return result;
            }
        }
        private byte[] To7BitEncodedInt(int value)
        {
            using (var code = new FlashWriter())
            {
                code.Write7BitEncodedInt(value);
                return code.ToArray();
            }
        }
    }
}