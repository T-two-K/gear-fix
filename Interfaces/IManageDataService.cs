using GearFix.Models.AppModels;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;

namespace GearFix.Interfaces
{
    public interface IManageDataService
    {
        public Task<JsonDataModel> LoadDataAsync();
        public Task<bool> TryLoadDataAsync(string password);
        public Task<bool> CheckApiKeys();
        public Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
        public Task<bool> SaveDataAsync(List<CarModel> cars, List<string> apiKeys);
        public Task<bool> CreatePassword(string password);
        public Task SaveApiKeyAsync(string newApiKey);
        public JsonDataModel? DeserializeData(string decryptedText);
        public string SerializeData(JsonDataModel jsonData);
        public string ChangeImagePath(string carImagePath);
        public bool CheckFileExistence();   
        public void DeleteOldImagePath(string oldImagePath);
        public void DeleteAllDataAndCloseApp();
    }
}
