using GearFix.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace GearFix.Services
{
    public class EncryptionService : IEncryptionService
    {
        private int _nonceSize = 12;
        private int _tagSize = 16;

        public byte[] Encrypt(byte[] hash, string jsonData)
        {
            byte[] nonce = RandomNumberGenerator.GetBytes(_nonceSize);
            byte[] plainText = Encoding.UTF8.GetBytes(jsonData);
            byte[] encryptedText = new byte[plainText.Length];
            byte[] tag = new byte[_tagSize];

            AesGcm aes = new AesGcm(hash, _tagSize);
            aes.Encrypt(nonce, plainText, encryptedText, tag);

            byte[] result = new byte[_nonceSize + encryptedText.Length + _tagSize];

            Buffer.BlockCopy(nonce, 0, result, 0, _nonceSize);
            Buffer.BlockCopy(encryptedText, 0, result, _nonceSize, encryptedText.Length);
            Buffer.BlockCopy(tag, 0, result, _nonceSize + encryptedText.Length, _tagSize);

            return result;
        }

        public string Decrypt(byte[] hash, byte[] encryptDataWithoutSalt)
        {
            byte[] nonce = new byte[_nonceSize];
            byte[] tag = new byte[_tagSize];
            byte[] encryptedText = new byte[encryptDataWithoutSalt.Length - _nonceSize - _tagSize];
            byte[] plainText = new byte[encryptedText.Length];

            Buffer.BlockCopy(encryptDataWithoutSalt, 0, nonce, 0, nonce.Length);
            Buffer.BlockCopy(encryptDataWithoutSalt, nonce.Length, encryptedText, 0, encryptedText.Length);
            Buffer.BlockCopy(encryptDataWithoutSalt, nonce.Length + encryptedText.Length, tag, 0, tag.Length);

            AesGcm aes = new AesGcm(hash, _tagSize);
            aes.Decrypt(nonce, encryptedText, tag, plainText);

            string jsonData = Encoding.UTF8.GetString(plainText);

            return jsonData;
        }
    }
}
