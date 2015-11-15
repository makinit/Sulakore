using System;
using System.Numerics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Sulakore.Protocol.Encryption
{
    public class HKeyExchange : IDisposable
    {
        protected BigInteger D { get; }
        protected BigInteger Modulus { get; }
        protected BigInteger Exponent { get; }

        protected BigInteger DHPrime { get; private set; }
        protected BigInteger DHGenerator { get; private set; }

        public RSACryptoServiceProvider RSA { get; }

        public bool IsDisposed { get; private set; }
        public bool CanDecrypt => (D != BigInteger.Zero);

        protected HKeyExchange(RSACryptoServiceProvider rsa)
        {
            RSA = rsa;

            RSAParameters keys =
                rsa.ExportParameters(true);

            D = new BigInteger(keys.D);
            Modulus = new BigInteger(keys.Modulus);
            Exponent = new BigInteger(keys.Exponent);
        }

        public HKeyExchange(int exponent, string modulus) :
            this(exponent, modulus, string.Empty)
        { }
        public HKeyExchange(int exponent, string modulus, string d)
        {
            var keys = new RSAParameters();

            Exponent = new BigInteger(exponent);
            keys.Exponent = Exponent.ToByteArray();

            Modulus = BigInteger.Parse("0" + modulus, NumberStyles.HexNumber);
            keys.Modulus = Modulus.ToByteArray();
            Array.Reverse(keys.Modulus);

            if (!string.IsNullOrWhiteSpace(d))
            {
                D = BigInteger.Parse("0" + d, NumberStyles.HexNumber);
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

        protected BigInteger Verify(string value)
        {
            var signed = BigInteger.Parse(value, NumberStyles.HexNumber);
            BigInteger padded = CalculatePublic(signed);

            var paddedData = padded.ToByteArray();
            Array.Reverse(paddedData);

            var paddedString = Encoding.UTF8.GetString(paddedData);
            int paddingEnd = paddedString.IndexOf('\0');

            return BigInteger.Parse(paddedString.Substring(paddingEnd + 1));
        }
        protected void CreateDHPrimes(int keySize)
        {

        }
        protected void CreateDHPublic(BigInteger dhPrime, BigInteger dhGenerator)
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

        public BigInteger CalculatePrivate(BigInteger value) =>
            BigInteger.ModPow(value, D, Modulus);

        public BigInteger CalculatePublic(BigInteger value) =>
            BigInteger.ModPow(value, Exponent, Modulus);

        public static HKeyExchange Create(int keySize)
        {
            var rsa = new RSACryptoServiceProvider(keySize);
            rsa.ExportParameters(true);

            return new HKeyExchange(rsa);
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