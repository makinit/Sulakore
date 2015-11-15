using System;
using System.Text;

namespace Sulakore.Protocol.Encryption
{
    public class HKeyExchange2 : IDisposable
    {
        private string _publicKey;
        private string _signedPrime;
        private string _signedGenerator;
        private readonly Random _byteGen;

        public RsaKey Rsa { get; }
        public BigInteger2 DhPrime { get; private set; }
        public BigInteger2 DhPublic { get; private set; }
        public BigInteger2 DhPrivate { get; private set; }
        public BigInteger2 DhGenerator { get; private set; }

        public int DHBitSize { get; }
        public bool IsInitiator { get; }
        public bool IsDisposed { get; private set; }

        public HKeyExchange2(int e, string n, int bitSize = 16)
            : this(e, n, null, bitSize)
        { }
        public HKeyExchange2(int e, string n, string d, int bitSize = 16)
        {
            _byteGen = new Random();

            DHBitSize = bitSize;
            IsInitiator = !string.IsNullOrWhiteSpace(d);

            Rsa = IsInitiator
                ? RsaKey.ParsePrivateKey(e, n, d)
                : RsaKey.ParsePublicKey(e, n);

            if (IsInitiator)
            {
                do { DhPrime = BigInteger2.GenPseudoPrime(212, 6, _byteGen); }
                while (!DhPrime.IsProbablePrime());

                do { DhGenerator = BigInteger2.GenPseudoPrime(212, 6, _byteGen); }
                while (DhGenerator >= DhPrime && !DhPrime.IsProbablePrime());

                if (DhGenerator > DhPrime)
                {
                    BigInteger2 dhGenShell = DhGenerator;
                    DhGenerator = DhPrime;
                    DhPrime = dhGenShell;
                }

                DhPrivate = new BigInteger2(RandomHex(30), bitSize);
                DhPublic = DhGenerator.ModPow(DhPrivate, DhPrime);
            }
        }

        public string GetPublicKey()
        {
            if (!string.IsNullOrEmpty(_publicKey))
                return _publicKey;

            byte[] publicKeyAsBytes = Encoding.Default.GetBytes(DhPublic.ToString(10));
            if (IsInitiator) Rsa.Sign(ref publicKeyAsBytes);
            else Rsa.Encrypt(ref publicKeyAsBytes);

            return (_publicKey = BytesToHex(publicKeyAsBytes).ToLower());
        }
        public string GetSignedPrime()
        {
            if (!IsInitiator || !string.IsNullOrEmpty(_signedPrime))
                return _signedPrime;

            byte[] primeAsBytes = Encoding.Default.GetBytes(DhPrime.ToString(10));
            Rsa.Sign(ref primeAsBytes);

            return (_signedPrime = BytesToHex(primeAsBytes).ToLower());
        }
        public string GetSignedGenerator()
        {
            if (!IsInitiator || !string.IsNullOrEmpty(_signedGenerator))
                return _signedGenerator;

            byte[] generatorAsBytes = Encoding.Default.GetBytes(DhGenerator.ToString(10));
            Rsa.Sign(ref generatorAsBytes);

            return (_signedGenerator = BytesToHex(generatorAsBytes).ToLower());
        }
        public byte[] GetSharedKey(string publicKey)
        {
            byte[] paddedPublicKeyAsBytes = HexToBytes(publicKey);
            if (IsInitiator) Rsa.Decrypt(ref paddedPublicKeyAsBytes);
            else Rsa.Verify(ref paddedPublicKeyAsBytes);

            publicKey = Encoding.Default
                .GetString(paddedPublicKeyAsBytes);

            var unpaddedPublicKey = new BigInteger2(publicKey, 10);
            return unpaddedPublicKey.ModPow(DhPrivate, DhPrime).ToBytes();
        }

        public void DoHandshake(string signedPrime, string signedGenerator)
        {
            if (IsInitiator) return;

            byte[] signedPrimeAsBytes = HexToBytes(signedPrime);
            Rsa.Verify(ref signedPrimeAsBytes);

            byte[] signedGeneratorAsBytes = HexToBytes(signedGenerator);
            Rsa.Verify(ref signedGeneratorAsBytes);

            DhPrime = new BigInteger2(Encoding.Default.GetString(signedPrimeAsBytes), 10);
            DhGenerator = new BigInteger2(Encoding.Default.GetString(signedGeneratorAsBytes), 10);

            if (DhPrime <= 2)
                throw new Exception("Prime cannot be <= 2!\nPrime: " + DhPrime);

            if (DhGenerator >= DhPrime)
            {
                throw new Exception(
                    $"Generator cannot be >= Prime!\nPrime: {DhPrime}\nGenerator: {DhGenerator}");
            }

            DhPrivate = new BigInteger2(RandomHex(30), DHBitSize);
            DhPublic = DhGenerator.ModPow(DhPrivate, DhPrime);
        }

        public static byte[] HexToBytes(string hex)
        {
            int hexLength = hex.Length;
            var data = new byte[hexLength / 2];
            for (int i = 0; i < hexLength; i += 2)
                data[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return data;
        }
        public static string BytesToHex(byte[] data)
        {
            return BitConverter.ToString(data)
                .Replace("-", string.Empty);
        }
        private string RandomHex(int length = 16)
        {
            string hex = string.Empty;
            for (int i = 0; i < length; i++)
            {
                var generated = (byte)_byteGen.Next(0, 256);
                hex += Convert.ToString(generated, 16);
            }
            return hex;
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
                Rsa?.Dispose();
                DhPrime?.Dispose();
                DhGenerator?.Dispose();
                DhPublic?.Dispose();
                DhPrivate?.Dispose();
            }
            IsDisposed = true;
        }
    }
}