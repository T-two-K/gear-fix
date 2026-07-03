using GearFix.Models.ApiModels.AiModels;
using GearFix.Models.ApiModels.GisModels;
using GearFix.Models.ApiModels.NhtsaModels;
using GearFix.Models.AppModels;

namespace GearFix.Interfaces
{
    public interface IApiService
    {
        public Task<bool> TryConnectToGeminiAsync(string apiKey);

        public Task<GeminiResponse> GeminiPost(
            GeminiRequest requestBody,
            string aiModel,
            string apiKey);

        public Task<NhtsaResponse> NhtsaGet(CarModel car);

        public Task<List<CarServiceInfo>> GISGetByLocation(double lat, double lon, int currentPage, int radiusM = 10000);

        public Task<List<CarServiceInfo>> GISGet(string searchQuery, int currentPage, int radiusM = 10000);

        public Task CheckInternetConnection();
    }
}
