using GearFix.Interfaces;
using System.IO;

namespace GearFix.Services
{
    public class FileService : IFileService
    {
        static private readonly string _appDataFolderPath =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        static private readonly string _appMainFolderName = "GearFix";

        static private readonly string _imagesDirectoryPath =
            Path.Combine(_appDataFolderPath, _appMainFolderName, "CarsImages");

        static private readonly List<string> _projectFolders = new()
        {
            _imagesDirectoryPath,
        };

        private string _mainFilePath = Path.Combine(_appDataFolderPath, _appMainFolderName, "Data.gearfix");

        public async Task<byte[]> LoadDataAsync()
        {
            if (!File.Exists(_mainFilePath)) return Array.Empty<byte>();

            using (FileStream fs = File.OpenRead(_mainFilePath))
            {
                byte[] buffer = new byte[fs.Length];

                await fs.ReadExactlyAsync(buffer);

                return buffer;
            }

        }

        public async Task<bool> SaveDataAsync(byte[] encryptedData)
        {
            await File.WriteAllBytesAsync(_mainFilePath, encryptedData);
            return true;
        }

        public bool DeleteFile()
        {
            if (!File.Exists(_mainFilePath)) return false;

            File.Delete(_mainFilePath);
            return true;
        }

        public bool IsFileExists()
        {
            if (File.Exists(_mainFilePath)) return true;

            return false;
        }

        public bool CreateNesesaryDirectory()
        {
            foreach (var directoryFullPath in _projectFolders)
            {
                if (!Directory.Exists(directoryFullPath))
                    Directory.CreateDirectory(directoryFullPath);
            }

            return true;
        }

        public string ChangeImagePath(string oldPath)
        {
            string newPath = Path.Combine(_imagesDirectoryPath, $"{Path.GetFileName(oldPath)}");

            File.Copy(oldPath, newPath, true);

            return newPath;
        }
    }
}
