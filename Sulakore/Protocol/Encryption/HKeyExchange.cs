using System.Text;
using System.Security.Cryptography;

namespace Sulakore.Protocol.Encryption
{
    public class HKeyExchange
    {
        public RSACryptoServiceProvider RSA { get; }

        public HKeyExchange(RSACryptoServiceProvider rsa)
        {
            RSA = rsa;
        }
        public HKeyExchange(int exponent, string modulus) :
            this(exponent, modulus, string.Empty)
        { }
        public HKeyExchange(int exponent, string modulus, string privateExponent)
        {

        }

        public static HKeyExchange Create(int keySize)
        {
            var rsa = new RSACryptoServiceProvider(keySize);
            rsa.ExportParameters(true);

            return new HKeyExchange(rsa);
        }
    }
}