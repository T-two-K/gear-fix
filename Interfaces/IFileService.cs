using GearFix.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearFix.Interfaces
{
    public interface IFileService
    {
        public Task<byte[]> LoadDataAsync();
        public Task<bool> SaveDataAsync(byte[] encryptedData);
        public bool DeleteFile();
        public string ChangeImagePath(string oldPath);
        public bool IsFileExists();
        public bool CreateNesesaryDirectory();

    }
}
