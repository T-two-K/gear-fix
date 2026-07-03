namespace GearFix.Interfaces
{
    public interface IHashService
    {
        public void CreateHash(string password);
        public void LoadHashInMemory(string password, byte[] encryptData);
        public void AddSaltIn(ref byte[] encryptedDataWithoutSalt);
        public void RemoveSaltFromData(ref byte[] encryptData);
        public bool IsHashLoaded();
        public bool CompareHashes(string unknownPassword);
        public byte[] GetHash();
        public byte[] GetSalt();
    }
}
