using GearFix.Interfaces;
using GearFix.Models.ApiModels.AiModels;
using GearFix.Models.ApiModels.GisModels;
using GearFix.Models.ApiModels.NhtsaModels;
using GearFix.Models.AppModels;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using System.Web;

namespace GearFix.Services
{
    public class ApiService : IApiService
    {
        private string _gisBaseUrl = "https://catalog.api.2gis.com/3.0/items";


        public static readonly HttpClient httpClient = new HttpClient()
        {
            Timeout = new TimeSpan(0, 0, 25)
        };

        private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };

        public async Task<GeminiResponse> GeminiPost(
            GeminiRequest requestBody,
            string aiModel,
            string apiKey)
        {
            await CheckInternetConnection();

            if (string.IsNullOrWhiteSpace(aiModel) || string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidDataException("Вы не выбрали модель или не указали api-ключ!");

            string urlGemini = $"https://generativelanguage.googleapis.com/v1beta/models/{aiModel}:generateContent?key={apiKey}";

            string requestJsonFormat = JsonSerializer.Serialize(requestBody, _jsonOptions);

            var geminiRequest = new StringContent(requestJsonFormat, Encoding.UTF8, "application/json");

            HttpResponseMessage geminiResponse = await httpClient.PostAsync(urlGemini, geminiRequest);

            if (!geminiResponse.IsSuccessStatusCode) await ThrowErrorAsync(geminiResponse);

            string jsonGeminiSuccessResponse = await geminiResponse.Content.ReadAsStringAsync();

            GeminiResponse deserializedGeminiResponse = JsonSerializer.Deserialize<GeminiResponse>(jsonGeminiSuccessResponse)
                ?? throw new InvalidOperationException("Не удалось десериализовать тело ответа Gemini");

            return deserializedGeminiResponse;
        }

        public async Task<List<CarServiceInfo>> GISGet(string searchQuery, int currentPage, int radius = 10000)
        {
            await CheckInternetConnection();
            var query = HttpUtility.ParseQueryString(string.Empty);

            string? gisApiKey = GetGisApiKey();

            if (string.IsNullOrWhiteSpace(gisApiKey))
                throw new InvalidOperationException("API ключ 2ГИС не был найден!");

            query["key"] = gisApiKey;
            query["q"] = searchQuery;
            query["radius"] = radius.ToString();
            query["type"] = "branch";
            query["fields"] = "items.point,items.schedule,items.rubrics,items.name";
            query["page_size"] = "10";
            query["page"] = currentPage.ToString();

            string uri = $"{_gisBaseUrl}?{query}";

            HttpResponseMessage response = await httpClient.GetAsync(uri);

            string result = await response.Content.ReadAsStringAsync();

            GisResponseBody gisResponseBody = JsonSerializer.Deserialize<GisResponseBody>(result)
                ?? throw new InvalidOperationException("Не удалось десериализовать данные о СТО.");

            return gisResponseBody.Result.Items;
        }

        public async Task<List<CarServiceInfo>> GISGetByLocation(double lat, double lon, int currentPage, int radiusKm = 10)
        {
            await CheckInternetConnection();
            var query = HttpUtility.ParseQueryString(string.Empty);

            string? gisApiKey = GetGisApiKey();

            if (string.IsNullOrWhiteSpace(gisApiKey))
                throw new InvalidOperationException("API ключ 2ГИС не был найден!");

            radiusKm *= 1000; // Конвертируем КМ в М

            query["key"] = gisApiKey;
            query["q"] = "автосервис, ремонт автомобилей, диагностика автомобилей, СТО, шиномонтаж, автомастерская";
            //Передаём их в обратном порядке, т.к. это особенности самой 2ГИС 
            query["point"] = $"{lon.ToString(CultureInfo.InvariantCulture)},{lat.ToString(CultureInfo.InvariantCulture)}";
            query["radius"] = radiusKm.ToString();
            query["type"] = "branch";
            query["fields"] = "items.point,items.schedule,items.rubrics,items.name";
            query["page_size"] = "10";
            query["page"] = currentPage.ToString();

            string uri = $"{_gisBaseUrl}?{query}";

            HttpResponseMessage response = await httpClient.GetAsync(uri);

            string result = await response.Content.ReadAsStringAsync();

            GisResponseBody gisResponseBody = JsonSerializer.Deserialize<GisResponseBody>(result)
                ?? throw new InvalidOperationException("Не удалось десериализовать данные о СТО.");

            return gisResponseBody.Result.Items;
        }

        public async Task<NhtsaResponse> NhtsaGet(CarModel car)
        {
            await CheckInternetConnection();
            string urlNhtsa = $"https://api.nhtsa.gov/complaints/complaintsByVehicle?make={car.Make.ToLower().Trim()}&model={car.Model.ToLower().Trim()}&modelYear={car.Year}";

            HttpResponseMessage nhtsaAnswer = await httpClient.GetAsync(urlNhtsa);

            string jsonData = await nhtsaAnswer.Content.ReadAsStringAsync();

            NhtsaResponse deserializedData = JsonSerializer.Deserialize<NhtsaResponse>(jsonData) ??
                throw new InvalidOperationException("Пришло неверное тело ответа (NhtsaGet)");

            return deserializedData;
        }

        public async Task<bool> TryConnectToGeminiAsync(string apiKey)
        {
            await CheckInternetConnection();
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite:generateContent?key={apiKey}";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent
                (
                    """{"contents":[{"parts":[{"text":"hi"}]}]}""",
                    Encoding.UTF8,
                    "application/json"
                )
            };

            HttpResponseMessage message = await httpClient.SendAsync(request);

            if (message.IsSuccessStatusCode) return true;

            string errorJson = await message.Content.ReadAsStringAsync();
            GeminiErrorResponse errorResponse = JsonSerializer.Deserialize<GeminiErrorResponse>(errorJson, _jsonOptions) ??
                throw new InvalidOperationException("Не удалось преобразовать данные в Json (TryConnectToGemini)");

            if (errorResponse.Error.Status == "FAILED_PRECONDITION")
                throw new InvalidOperationException("Не удалось подключиться к Gemini," +
                    " т.к. она не поддерживается в вашем регионе (Рекомендую влючить VPN).");

            if (errorResponse.Error.Status == "INVALID_ARGUMENT")
                throw new InvalidOperationException("Вы передали некоректный API ключ, пожалуйста, введите ключ," +
                    " который является вашим и будет валидным - это необходимо для корректной работы приложения");

            throw new Exception($"Произошло необработанное исключение: {errorJson}");
        }

        private async Task ThrowErrorAsync(HttpResponseMessage geminiResponse)
        {
            string jsonError = await geminiResponse.Content.ReadAsStringAsync();

            GeminiErrorResponse geminiErrorResponse = JsonSerializer.Deserialize<GeminiErrorResponse>(jsonError, _jsonOptions)
                ?? throw new InvalidOperationException("Не удалось десериализовать данные об ошибке!");

            bool notStatusProblem = geminiErrorResponse.Error.Status switch
            {
                "FAILED_PRECONDITION" => throw new InvalidOperationException("Нейросеть не поддерживается в вашем регионе!" +
                                                    " (Рекомендую включить VPN и попробовать снова)."),

                "INVALID_ARGUMENT" => throw new InvalidOperationException("Было переданно неверное тело запроса или невалидные" +
                                                    " данные, пожалуйста попробуйте ещё раз!"),

                "QUOTA" => throw new InvalidOperationException("У вас закончились токены! Вы можете выбрать другую модель," +
                                                    " для продолжения диалога!"),
                _ => true
            };

            bool notStatusCodeProblem = geminiErrorResponse.Error.Code switch
            {
                401 => throw new InvalidOperationException("Был передан неверный (несуществующий) API ключ," +
                                            " пожалуйста, проверьте его валидность!"),

                429 => throw new InvalidOperationException("Вы слишком часто оброщались к нейросети," +
                                            " пожалуйста, подождите минуту и попробуйте после."),

                503 => throw new InvalidOperationException("К сожалению, сервера данной модели перегруженны," +
                                            " пожалуйста, повторите свой запрос через минуту!"),
                _ => true
            };

            throw new Exception($"Не обработанное исключение, тело ответа: {jsonError}");
        }

        private string? GetGisApiKey()
        {
            if (!File.Exists("GISkey.json"))
                throw new InvalidOperationException("Файл GISkey.json не найден!");

            string gisKeyJson = File.ReadAllText("GISkey.json");

            JsonNode? node = JsonNode.Parse(gisKeyJson);

            if (node != null && node["gisApiKey"]?.GetValue<string>() != null)
                return node["gisApiKey"]?.GetValue<string>();

            return null;
        }

        public async Task CheckInternetConnection()
        {
            try
            {
                Ping ping = new();
                PingReply reply = await ping.SendPingAsync("8.8.8.8", 5000);
                if(reply.Status == IPStatus.Success)
                    return;

                throw new InvalidOperationException("Вы не подключены к интернету.");
            }
            catch
            {
                throw new InvalidOperationException("Вы не подключены к интернету.");
            }
        }
    }
}
