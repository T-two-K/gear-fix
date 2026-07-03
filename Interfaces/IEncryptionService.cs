using System;
using System.Collections.Generic;
using System.Text;

namespace GearFix.Interfaces
{
    public interface IEncryptionService
    {
        public byte[] Encrypt(byte[] hash, string jsonData);
        public string Decrypt(byte[] hash, byte[] encryptDataWithoutSalt);
    }
}
