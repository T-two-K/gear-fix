using GearFix.DTO;
using GearFix.Interfaces;
using GearFix.Models.ApiModels.GisModels;
using Microsoft.Web.WebView2.Wpf;
using System.Globalization;
using System.Text.Json;

namespace GearFix.Services
{
    public class MapService : IMapService
    {
        private WebView2 _webView;

        public MapService(WebView2 webView)
        {
            _webView = webView;
        }

        public async Task DrawRaiusCircle(double lat, double lon, int radiusKm)
        {
            string correctLat = lat.ToString(CultureInfo.InvariantCulture);
            string correctLon = lon.ToString(CultureInfo.InvariantCulture);

            await _webView.CoreWebView2.ExecuteScriptAsync($"drawRadiusCircle({correctLat}, {correctLon}, {radiusKm})");
        }

        public async Task PlaceCarServices(ICollection<CarServiceInfo> services)
        {
            string jsonString = JsonSerializer.Serialize(services);

            await _webView.CoreWebView2.ExecuteScriptAsync($"placeCarServices({jsonString})");
        }

        public async Task ShowUserLocation(double lat, double lon, int radiusKm)
        {
            string correctLat = lat.ToString(CultureInfo.InvariantCulture);
            string correctLon = lon.ToString(CultureInfo.InvariantCulture);

            await _webView.CoreWebView2.ExecuteScriptAsync($"showUserLocation({correctLat}, {correctLon}, {radiusKm})");
        }

        public async Task ClearAllMap()
        {
            await _webView.CoreWebView2.ExecuteScriptAsync($"clearAll()");
        }

        public async Task ExecuteGettingUserLocation(int radius)
        {
            await _webView.CoreWebView2.ExecuteScriptAsync($"getUserLocation({radius})");
        }
    }
}
