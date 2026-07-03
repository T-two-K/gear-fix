using GearFix.Interfaces;
using GearFix.Models.AppModels;
using System.IO;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;


//Окей три базовых сервиса ты реализовал (хеширование, шифрование, работа с файлом), теперь необходимо всё это склеить.
//Собственно здесь то ты и потрудишься))).
namespace GearFix.Services
{
    public class ManageDataService : IManageDataService
    {
        private readonly JsonSerializerOptions jsOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };

        private IFileService _fileService;
        private IEncryptionService _encryptionService;
        private IHashService _hashService;

        public ManageDataService(
            IFileService fileService,
            IEncryptionService encryptionService,
            IHashService hashService)
        {
            _fileService = fileService;
            _encryptionService = encryptionService;
            _hashService = hashService;
        }

        public async Task<bool> TryLoadDataAsync(string password)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    byte[] encryptedData = await _fileService.LoadDataAsync();

                    _hashService.LoadHashInMemory(password, encryptedData);
                    _hashService.RemoveSaltFromData(ref encryptedData);

                    if (encryptedData.Length == 0)
                        return true;

                    _encryptionService.Decrypt(_hashService.GetHash(), encryptedData);

                    return true;
                }
                catch (AuthenticationTagMismatchException)
                {
                    throw new InvalidOperationException("Вы ввели неверный пароль! Пожалуйста попробуйте ещё раз!");
                }
                catch (CryptographicException)
                {
                    throw new InvalidOperationException("Произошла ошибка при дешифровке файла," +
                        " возможно данные повреждены или изменены!");
                }
                catch (ArgumentException)
                {
                    throw new InvalidOperationException("Были переданны невалидные данные!");
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        public async Task<JsonDataModel> LoadDataAsync()
        {
            try
            {
                return await Task.Run(async () =>
                {
                    byte[] encryptedData = await _fileService.LoadDataAsync();

                    if (encryptedData.Length == 0)
                        return new JsonDataModel();

                    _hashService.RemoveSaltFromData(ref encryptedData);

                    string loadedDataInJson = _encryptionService.Decrypt(_hashService.GetHash(), encryptedData);

                    JsonDataModel? result = DeserializeData(loadedDataInJson);

                    if (result == null)
                        return new JsonDataModel();

                    return result;
                });
            }
            catch (AuthenticationTagMismatchException)
            {
                throw new InvalidOperationException("Кажется кто-то поигрался с моим кодом, как не красиво.");
            }
            catch (CryptographicException)
            {
                throw new InvalidOperationException("Произошла ошибка при дешифровке файла," +
                    " возможно данные повреждены или изменены!");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> CheckApiKeys()
        {
            JsonDataModel model = await LoadDataAsync();
            if (model.Keys == null || model.Keys.Count == 0)
                return false;

            return true;
        }

        public async Task SaveApiKeyAsync(string newApiKey)
        {
            JsonDataModel model = await LoadDataAsync();
            model.Keys.Add(newApiKey);
            await SaveDataAsync(model.Cars, model.Keys);
        }

        public async Task<bool> SaveDataAsync(List<CarModel> cars, List<string> apiKeys)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    if (_hashService.IsHashLoaded())
                    {
                        JsonDataModel savedData = new JsonDataModel()
                        {
                            Cars = cars,
                            Keys = apiKeys
                        };

                        string savedJsonData = SerializeData(savedData);

                        byte[] encryptData = _encryptionService.Encrypt(_hashService.GetHash(), savedJsonData);
                        _hashService.AddSaltIn(ref encryptData);

                        if (!await _fileService.SaveDataAsync(encryptData))
                            throw new InvalidOperationException("Данные не были сохранены!");

                        return true;
                    }
                    else
                    {
                        throw new InvalidOperationException("Хэш не был создан!");
                    }
                }
                catch (CryptographicException)
                {
                    throw new InvalidOperationException("Произошла ошибка при шифровании данных!");
                }
                catch (ArgumentException)
                {
                    throw new InvalidOperationException("Были переданы невалидные данные!");
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            if (_hashService.CompareHashes(currentPassword))
            {
                JsonDataModel jsonData = await LoadDataAsync();

                _hashService.CreateHash(newPassword);
                await SaveDataAsync(jsonData.Cars, jsonData.Keys);
                return true;
            }

            throw new InvalidOperationException("Пароли не совпадают! Введите текущий пароль ещё раз!");
        }

        public void DeleteAllDataAndCloseApp()
        {
            if (_fileService.DeleteFile())
            {
                App.Current.Shutdown();
            }
        }

        public async Task<bool> CreatePassword(string password)
        {
            _hashService.CreateHash(password);
            await SaveDataAsync(new List<CarModel>(), new List<string>());
            return true;
        }

        public string ChangeImagePath(string oldPath) =>
            _fileService.ChangeImagePath(oldPath);

        public void DeleteOldImagePath(string oldImagePath)
        {
            if (File.Exists(oldImagePath))
                File.Delete(oldImagePath);
        }

        public bool CheckFileExistence() =>
            _fileService.IsFileExists();

        public JsonDataModel? DeserializeData(string decryptedText) =>
            JsonSerializer.Deserialize<JsonDataModel>(decryptedText, jsOptions);

        public string SerializeData(JsonDataModel jsonData) =>
            JsonSerializer.Serialize(jsonData, jsOptions);
    }
}
