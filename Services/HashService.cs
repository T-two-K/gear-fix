using GearFix.Interfaces;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;

namespace GearFix.Services
{
    public class HashService : IHashService
    {
        private int _saltSize = 16;
        private int _keySize = 32;
        private int _memory = 65536;
        private int _iterations = 4;
        private int _degreeOfParallelism = 8;

        private byte[] _hashSalt = Array.Empty<byte>();

        public void CreateHash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(_saltSize);

            using var argon = CreateArgon(password, salt);
            byte[] hash = argon.GetBytes(_keySize);

            SaveHashAndSalt(hash, salt);
        }

        //Получаем данные через параметр, удаляем соль из массива и сохраняем хэш и соль
        public void LoadHashInMemory(string password, byte[] encryptData)
        {
            if (encryptData == null)
                throw new InvalidOperationException("Данные были пустыми!");

            if (encryptData.Length <= _saltSize)
                throw new InvalidOperationException("Неверные данные!");

            byte[] salt = new byte[_saltSize];
            Buffer.BlockCopy(encryptData, encryptData.Length - _saltSize, salt, 0, _saltSize);

            using var argon = CreateArgon(password, salt);
            byte[] hash = argon.GetBytes(_keySize);

            SaveHashAndSalt(hash, salt);
        }

        public bool CompareHashes(string unknownPassword)
        {
            if(IsHashLoaded())
            {
                byte[] salt = GetSalt();
                Argon2id argon = CreateArgon(unknownPassword, salt);

                byte[] unknownHash = argon.GetBytes(_keySize);
                return CryptographicOperations.FixedTimeEquals(unknownHash, GetHash());
            }

            throw new InvalidOperationException("Хэш не загружен в память! Проверка хэшей невозможна!");
        }

        public void RemoveSaltFromData(ref byte[] encryptData) =>
            Array.Resize(ref encryptData, encryptData.Length - _saltSize);

        private void SaveHashAndSalt(byte[] hash, byte[] salt)
        {
            if (_hashSalt.Length == _keySize + _saltSize)
                Array.Clear(_hashSalt, 0, _hashSalt.Length);
            else
                _hashSalt = new byte[_keySize + _saltSize];

            Buffer.BlockCopy(hash, 0, _hashSalt, 0, _keySize);
            Buffer.BlockCopy(salt, 0, _hashSalt, _keySize, _saltSize);
        }

        public void AddSaltIn(ref byte[] encryptedDataWithoutSalt)
        {
            byte[] salt = GetSalt();

            int originalLength = encryptedDataWithoutSalt.Length;
            Array.Resize(ref encryptedDataWithoutSalt, originalLength + _saltSize);

            Buffer.BlockCopy(salt, 0, encryptedDataWithoutSalt, originalLength, _saltSize);
        }

        public bool IsHashLoaded()
        {
            if(_hashSalt == null || _hashSalt.Length < _saltSize + _keySize)
                return false;

            return true;
        }

        public byte[] GetHash()
        {
            CheckMassive();

            byte[] hash = new byte[_keySize];
            Buffer.BlockCopy(_hashSalt, 0, hash, 0, _keySize);
            return hash;
        }

        public byte[] GetSalt()
        {
            CheckMassive();

            byte[] salt = new byte[_saltSize];
            Buffer.BlockCopy(_hashSalt, _keySize, salt, 0, _saltSize);
            return salt;
        }

        private Argon2id CreateArgon(string password, byte[] salt)
        {
            return new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = _degreeOfParallelism,
                Iterations = _iterations,
                MemorySize = _memory
            };
        }

        private void CheckMassive()
        {
            if (_hashSalt == null || _hashSalt.Length == 0)
                throw new NullReferenceException("Хранилище хэша и соли пустое!");

            if (_hashSalt.Length != _saltSize + _keySize)
                throw new InvalidOperationException("Неверный размер массива (HashService)!");
        }
    }
}
