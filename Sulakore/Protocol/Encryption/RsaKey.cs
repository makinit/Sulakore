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
        public BigInteger2 E { get; }
        /// <summary>
        /// Public Modulus
        /// </summary>
        public BigInteger2 N { get; }
        /// <summary>
        /// Private Exponent
        /// </summary>
        public BigInteger2 D { get; }
        /// <summary>
        /// Secret Prime Factor (P * Q = N)
        /// </summary>
        public BigInteger2 P { get; }
        /// <summary>
        /// Secret Prime Factor (P * Q = N)
        /// </summary>
        public BigInteger2 Q { get; }
        /// <summary>
        /// D Mod (P - 1)
        /// </summary>
        public BigInteger2 Dmp1 { get; }
        /// <summary>
        /// D Mod (Q - 1)
        /// </summary>
        public BigInteger2 Dmq1 { get; }
        /// <summary>
        /// (Inverse)Q Mod P
        /// </summary>
        public BigInteger2 Iqmp { get; }
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

        public PKCSPadding Padding { get; set; }

        static RsaKey()
        {
            _byteGen = new Random();
        }
        public RsaKey(BigInteger2 e, BigInteger2 n) :
            this(e, n, null, null, null, null, null, null)
        { }
        public RsaKey(BigInteger2 e, BigInteger2 n, BigInteger2 d) :
            this(e, n, d, null, null, null, null, null)
        { }
        public RsaKey(BigInteger2 e, BigInteger2 n, BigInteger2 d, BigInteger2 p,
            BigInteger2 q, BigInteger2 dmp1, BigInteger2 dmq1, BigInteger2 iqmp)
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
        private void Decrypt(Func<BigInteger2, BigInteger2> doFunc, ref byte[] data)
        {
            var encryptedN = new BigInteger2(data);
            BigInteger2 result = doFunc(encryptedN);

            byte[] padded = result.ToBytes();
            data = PKCS1Unpad(padded, BlockSize);
        }

        public void Encrypt(ref byte[] data)
        {
            Encrypt(DoPublic, ref data);
        }
        private void Encrypt(Func<BigInteger2, BigInteger2> doFunc, ref byte[] data)
        {
            byte[] padded = PKCS1Pad(data, BlockSize);
            var paddedN = new BigInteger2(padded);

            data = doFunc(paddedN).ToBytes();
        }

        private BigInteger2 DoPrivate(BigInteger2 x)
        {
            if (P == null && Q == null)
                return x.ModPow(D, N);

            BigInteger2 xp = (x % P).ModPow(Dmp1, P);
            BigInteger2 xq = (x % Q).ModPow(Dmq1, Q);

            while (xp < xq) xp = xp + P;
            return ((((xp - xq) * (Iqmp)) % P) * Q) + xq;
        }
        private BigInteger2 DoPublic(BigInteger2 x) => x.ModPow(E, N);

        private byte[] PKCS1Pad(byte[] data, int length)
        {
            var buffer = new byte[length];
            Buffer.BlockCopy(data, 0, buffer,
                buffer.Length - data.Length, data.Length);

            buffer[1] = (byte)(Padding + 1);
            bool isRandom = (Padding == PKCSPadding.RandomByte);

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
            Padding = (PKCSPadding)(data[0] - 1);

            int position = 0;
            while (data[position++] != 0) ;

            var buffer = new byte[data.Length - position];
            Buffer.BlockCopy(data, position, buffer, 0, buffer.Length);

            return buffer;
        }

        public static RsaKey ParsePublicKey(int e, string n)
        {
            return new RsaKey(new BigInteger2(
                e.ToString(), 16), new BigInteger2(n, 16));
        }
        public static RsaKey Create(int exponent, int bitSize)
        {
            BigInteger2 p, q, e = new BigInteger2(exponent.ToString(), 16);

            BigInteger2 phi, p1, q1;
            int qs = bitSize >> 1;
            do
            {
                do p = BigInteger2.GenPseudoPrime(bitSize - qs, 6, _byteGen);
                while ((p - 1).Gcd(e) != 1 && !p.IsProbablePrime(10));

                do q = BigInteger2.GenPseudoPrime(qs, 6, _byteGen);
                while ((q - 1).Gcd(e) != 1 && !q.IsProbablePrime(10) && q == p);

                if (p < q)
                {
                    BigInteger2 tmpP = p;
                    p = q; q = tmpP;
                }
                phi = (p1 = (p - 1)) * (q1 = (q - 1));
            }
            while (phi.Gcd(e) != 1);

            BigInteger2 n = p * q;
            BigInteger2 d = e.ModInverse(phi);
            BigInteger2 dmp1 = d % p1;
            BigInteger2 dmq1 = d % q1;
            BigInteger2 iqmp = q.ModInverse(p);
            return new RsaKey(e, n, d, p, q, dmp1, dmq1, iqmp);
        }
        public static RsaKey ParsePrivateKey(int e, string n, string d)
        {
            return new RsaKey(new BigInteger2(e.ToString(), 16),
                new BigInteger2(n, 16), new BigInteger2(d, 16));
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
                E?.Dispose();
                N?.Dispose();
                D?.Dispose();
                P?.Dispose();
                Q?.Dispose();
                Dmp1?.Dispose();
                Iqmp?.Dispose();
            }
            IsDisposed = true;
        }
    }
}