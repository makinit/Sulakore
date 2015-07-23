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

namespace Sulakore.Protocol.Encryption
{
    public class RsaKey : IDisposable
    {
        private static readonly Random _byteGen;

        /// <summary>
        /// Gets the block size of the <see cref="RsaKey"/>.
        /// </summary>
        public int BlockSize { get; }
        /// <summary>
        /// Public Exponent
        /// </summary>
        public BigInteger E { get; }
        /// <summary>
        /// Public Modulus
        /// </summary>
        public BigInteger N { get; }
        /// <summary>
        /// Private Exponent
        /// </summary>
        public BigInteger D { get; }
        /// <summary>
        /// Secret Prime Factor (P * Q = N)
        /// </summary>
        public BigInteger P { get; }
        /// <summary>
        /// Secret Prime Factor (P * Q = N)
        /// </summary>
        public BigInteger Q { get; }
        /// <summary>
        /// D Mod (P - 1)
        /// </summary>
        public BigInteger Dmp1 { get; }
        /// <summary>
        /// D Mod (Q - 1)
        /// </summary>
        public BigInteger Dmq1 { get; }
        /// <summary>
        /// (Inverse)Q Mod P
        /// </summary>
        public BigInteger Iqmp { get; }
        /// <summary>
        /// Gets a value that determines whether the <see cref="RsaKey"/> can encrypt data.
        /// </summary>
        public bool CanEncrypt { get; }
        /// <summary>
        /// Gets a value that determines whether the <see cref="RsaKey"/> can decrypt data.
        /// </summary>
        public bool CanDecrypt { get; }
        /// <summary>
        /// Gets a value that determines whether the <see cref="RsaKey"/> has already been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        public PkcsPadding Padding { get; set; }

        static RsaKey()
        {
            _byteGen = new Random();
        }
        public RsaKey(BigInteger e, BigInteger n) :
            this(e, n, null, null, null, null, null, null)
        { }
        public RsaKey(BigInteger e, BigInteger n, BigInteger d) :
            this(e, n, d, null, null, null, null, null)
        { }
        public RsaKey(BigInteger e, BigInteger n, BigInteger d, BigInteger p,
            BigInteger q, BigInteger dmp1, BigInteger dmq1, BigInteger iqmp)
        {
            E = e;
            N = n;
            D = d;
            P = p;
            Q = q;
            Dmp1 = dmp1;
            Dmq1 = dmq1;
            Iqmp = iqmp;

            CanEncrypt = (e != null && n != null);
            CanDecrypt = (CanEncrypt && d != null);

            BlockSize = (N.BitCount() + 7) / 8;
        }

        public void Sign(ref byte[] data)
        {
            Encrypt(DoPrivate, ref data);
        }
        public void Verify(ref byte[] data)
        {
            Decrypt(DoPublic, ref data);
        }

        public void Decrypt(ref byte[] data)
        {
            Decrypt(DoPrivate, ref data);
        }
        private void Decrypt(Func<BigInteger, BigInteger> doFunc, ref byte[] data)
        {
            var encryptedN = new BigInteger(data);
            byte[] padded = doFunc(encryptedN).ToBytes();

            data = PKCS1Unpad(padded, BlockSize);
        }

        public void Encrypt(ref byte[] data)
        {
            Encrypt(DoPublic, ref data);
        }
        private void Encrypt(Func<BigInteger, BigInteger> doFunc, ref byte[] data)
        {
            byte[] padded = PKCS1Pad(data, BlockSize);
            var paddedN = new BigInteger(padded);

            data = doFunc(paddedN).ToBytes();
        }

        private BigInteger DoPrivate(BigInteger x)
        {
            if (P == null && Q == null)
                return x.ModPow(D, N);

            BigInteger xp = (x % P).ModPow(Dmp1, P);
            BigInteger xq = (x % Q).ModPow(Dmq1, Q);

            while (xp < xq) xp = xp + P;
            return ((((xp - xq) * (Iqmp)) % P) * Q) + xq;
        }
        private BigInteger DoPublic(BigInteger x) => x.ModPow(E, N);

        private byte[] PKCS1Pad(byte[] data, int length)
        {
            var buffer = new byte[length];
            Buffer.BlockCopy(data, 0, buffer,
                buffer.Length - data.Length, data.Length);
            
            buffer[1] = (byte)(Padding + 1);
            bool isRandom = (Padding == PkcsPadding.RandomByte);

            for (int i = 2; i < buffer.Length; i++)
            {
                if (buffer[i] == data[0])
                {
                    buffer[i - 1] = 0;
                    break;
                }
                else
                {
                    buffer[i] = isRandom ?
                        (byte)_byteGen.Next(1, 256) : byte.MaxValue;
                }
            }

            return buffer;
        }
        private byte[] PKCS1Unpad(byte[] data, int length)
        {
            Padding = (PkcsPadding)(data[0] - 1);

            int position = 0;
            while (data[position++] != 0) ;

            var buffer = new byte[data.Length - position];
            Buffer.BlockCopy(data, position, buffer, 0, buffer.Length);

            return buffer;
        }

        public static RsaKey ParsePublicKey(int e, string n)
        {
            return new RsaKey(new BigInteger(
                e.ToString(), 16), new BigInteger(n, 16));
        }
        public static RsaKey Create(int exponent, int bitSize)
        {
            BigInteger p, q, e = new BigInteger(exponent.ToString(), 16);

            BigInteger phi, p1, q1;
            int qs = bitSize >> 1;
            do
            {
                do p = BigInteger.GenPseudoPrime(bitSize - qs, 6, _byteGen);
                while ((p - 1).Gcd(e) != 1 && !p.IsProbablePrime(10));

                do q = BigInteger.GenPseudoPrime(qs, 6, _byteGen);
                while ((q - 1).Gcd(e) != 1 && !q.IsProbablePrime(10) && q == p);

                if (p < q)
                {
                    BigInteger tmpP = p;
                    p = q; q = tmpP;
                }
                phi = (p1 = (p - 1)) * (q1 = (q - 1));
            }
            while (phi.Gcd(e) != 1);

            BigInteger n = p * q;
            BigInteger d = e.ModInverse(phi);
            BigInteger dmp1 = d % p1;
            BigInteger dmq1 = d % q1;
            BigInteger iqmp = q.ModInverse(p);
            return new RsaKey(e, n, d, p, q, dmp1, dmq1, iqmp);
        }
        public static RsaKey ParsePrivateKey(int e, string n, string d)
        {
            return new RsaKey(new BigInteger(e.ToString(), 16),
                new BigInteger(n, 16), new BigInteger(d, 16));
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            if (disposing)
            {
                if (E != null)
                    E.Dispose();

                if (N != null)
                    N.Dispose();

                if (D != null)
                    D.Dispose();

                if (P != null)
                    P.Dispose();

                if (Q != null)
                    Q.Dispose();

                if (Dmp1 != null)
                    Dmp1.Dispose();

                if (Iqmp != null)
                    Iqmp.Dispose();
            }
            IsDisposed = true;

        }
    }
}