/*
    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    This file is part of the Sulakore library.
    Copyright (C) 2015 ArachisH
    
    This code is licensed under the GNU General Public License.
    See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;

namespace Sulakore.Protocol.Encryption
{
    public class Rc4
    {
        private int _i, _j;
        private readonly int[] _table;
        private readonly object _parseLock;

        public IReadOnlyList<byte> Key { get; }

        public Rc4(byte[] key)
        {
            Key = new List<byte>(key);

            _parseLock = new object();
            _table = new int[256];

            for (int i = 0; i < 256; i++)
                _table[i] = i;

            for (int j = 0, enX = 0; j < 256; j++)
                Swap(j, enX = (((enX + _table[j]) + (key[j % key.Length])) % 256));
        }

        public void Parse(byte[] data)
        {
            lock (_parseLock)
            {
                for (int k = 0; k < data.Length; k++)
                {
                    Swap(_i = (++_i % 256), _j = ((_j + _table[_i]) % 256));
                    data[k] ^= (byte)(_table[(_table[_i] + _table[_j]) % 256]);
                }
            }
        }
        public byte[] SafeParse(byte[] data)
        {
            var dataCopy = new byte[data.Length];
            Buffer.BlockCopy(data, 0, dataCopy, 0, data.Length);

            Parse(dataCopy);
            return dataCopy;
        }

        private void Swap(int a, int b)
        {
            int temp = _table[a];
            _table[a] = _table[b];
            _table[b] = temp;
        }
    }
}