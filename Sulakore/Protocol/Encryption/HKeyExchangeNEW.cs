using System;
using System.Numerics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Sulakore.Protocol.Encryption
{
    internal class HKeyExchangeNEW : IDisposable
    {
        protected System.Numerics.BigInteger D { get; }
        protected System.Numerics.BigInteger Modulus { get; }
        protected System.Numerics.BigInteger Exponent { get; }

        protected System.Numerics.BigInteger DHPrime { get; private set; }
        protected System.Numerics.BigInteger DHGenerator { get; private set; }

        public RSACryptoServiceProvider RSA { get; }

        public bool IsDisposed { get; private set; }
        public bool CanDecrypt => (D != System.Numerics.BigInteger.Zero);

        protected HKeyExchangeNEW(RSACryptoServiceProvider rsa)
        {
            RSA = rsa;

            RSAParameters keys =
                rsa.ExportParameters(true);

            D = new System.Numerics.BigInteger(keys.D);
            Modulus = new System.Numerics.BigInteger(keys.Modulus);
            Exponent = new System.Numerics.BigInteger(keys.Exponent);
        }

        public HKeyExchangeNEW(int exponent, string modulus) :
            this(exponent, modulus, string.Empty)
        { }
        public HKeyExchangeNEW(int exponent, string modulus, string d)
        {
            var keys = new RSAParameters();

            Exponent = new System.Numerics.BigInteger(exponent);
            keys.Exponent = Exponent.ToByteArray();

            Modulus = System.Numerics.BigInteger.Parse("0" + modulus, NumberStyles.HexNumber);
            keys.Modulus = Modulus.ToByteArray();
            Array.Reverse(keys.Modulus);

            if (!string.IsNullOrWhiteSpace(d))
            {
                D = System.Numerics.BigInteger.Parse("0" + d, NumberStyles.HexNumber);
                keys.D = D.ToByteArray();
                Array.Reverse(keys.D);
            }

            RSA = new RSACryptoServiceProvider();
            RSA.ImportParameters(keys);
        }

        public virtual void DoHandshake(string signedP, string signedG)
        {
            DHPrime = Verify(signedP);
            DHGenerator = Verify(signedG);
        }

        protected System.Numerics.BigInteger Verify(string value)
        {
            var signed = System.Numerics.BigInteger.Parse(value, NumberStyles.HexNumber);
            System.Numerics.BigInteger padded = CalculatePublic(signed);

            var paddedData = padded.ToByteArray();
            Array.Reverse(paddedData);

            var paddedString = Encoding.UTF8.GetString(paddedData);
            int paddingEnd = paddedString.IndexOf('\0');

            return System.Numerics.BigInteger.Parse(paddedString.Substring(paddingEnd + 1));
        }
        protected void CreateDHPrimes(int keySize)
        {

        }
        protected void CreateDHPublic(System.Numerics.BigInteger dhPrime, System.Numerics.BigInteger dhGenerator)
        { }

        protected byte[] HexToBytes(string value)
        {
            var data = new byte[value.Length / 2];
            for (int i = 0; i < value.Length; i += 2)
            {
                data[i / 2] = Convert.ToByte(
                    value.Substring(i, 2), 16);
            }
            return data;
        }
        protected string BytesToHex(byte[] value)
        {
            return BitConverter.ToString(value)
                .Replace("-", string.Empty);
        }

        public System.Numerics.BigInteger CalculatePrivate(System.Numerics.BigInteger value) =>
            System.Numerics.BigInteger.ModPow(value, D, Modulus);

        public System.Numerics.BigInteger CalculatePublic(System.Numerics.BigInteger value) =>
            System.Numerics.BigInteger.ModPow(value, Exponent, Modulus);

        public static HKeyExchangeNEW Create(int keySize)
        {
            var rsa = new RSACryptoServiceProvider(keySize);
            rsa.ExportParameters(true);

            return new HKeyExchangeNEW(rsa);
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
                RSA.Dispose();
            }
            IsDisposed = true;
        }
    }
}