using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GearFix.Interfaces;
using GearFix.Models.ApiModels.GisModels;
using GearFix.Models.AppModels;
using GearFix.ViewModels.DialogViewModels;
using System.IO;
using System.Text.Json;
using System.Windows;
using Point = GearFix.Models.ApiModels.GisModels.Point;

namespace GearFix.ViewModels
{
    public partial class MapViewModel : BaseViewModel
    {
        private readonly string _webFilesDirectoryName = "WebDesign";
        private const int _maxRecordsReturndForOneTime = 10;

        private IApiService _apiService;

        private Point _userCoordinate = new() { Lat = 53.9045, Lon = 27.5615 };

        public IMapService? MapService { private get; set; }

        public Uri WebViewUri { get; private set; } = new Uri("https://app.local/WebView.html");

        [ObservableProperty]
        private int _searchRadius = 1;
        [ObservableProperty]
        private int _maxResults = 1;
        [ObservableProperty]
        private string _searchQuery = string.Empty;

        public MapViewModel(
            IApiService apiService,
            IDialogManager dialogManager,
            IManageDataService manageDataService,
            INavigationService navigationService) : base(dialogManager, manageDataService, navigationService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        private async Task FindNearby()
        {
            await ExecuteSaveAsync(async () =>
            {
                if (MapService == null)
                    throw new InvalidOperationException("Один из сервисов не был загружен");

                if (MaxResults <= 0)
                    throw new InvalidOperationException("Максимальное количество мастерских, должно быть больше 0!");

                if (SearchRadius <= 0)
                    throw new InvalidOperationException("Радиус поиска, должен быть больше 0!");

                List<CarServiceInfo> carServicesInfo = new List<CarServiceInfo>();

                int maximumPages = MaxResults / _maxRecordsReturndForOneTime;
                int remainder = MaxResults % _maxRecordsReturndForOneTime;
                int currentPage = 0;

                if (remainder > 0)
                    maximumPages++;

                while (currentPage <= maximumPages)
                {
                    if (remainder > 0 && currentPage == maximumPages)
                    {
                        List<CarServiceInfo> lastList = await _apiService.GISGetByLocation(_userCoordinate.Lat, _userCoordinate.Lon, currentPage, SearchRadius);
                        carServicesInfo.AddRange(lastList.Take(remainder));
                        break;
                    }

                    carServicesInfo.AddRange(await _apiService.GISGetByLocation(_userCoordinate.Lat, _userCoordinate.Lon, currentPage, SearchRadius));
                    currentPage++;
                }

                if (carServicesInfo.Any())
                    DialogManager.ShowDialog<SuccessDialogViewModel>(new WarningDialogModel()
                    {
                        Title = "Успех!",
                        WarningContent = $"Количество найденых сервисов: {carServicesInfo.Count}",
                        ButtonContent = "Ок"
                    });
                else
                    DialogManager.ShowDialog<ErrorDialogViewModel>(new WarningDialogModel()
                    {
                        Title = "Ничего не найдено!",
                        WarningContent = $"Не было найдено ни одного сервиса по близости.",
                        ButtonContent = "Ок"
                    });

                await MapService.PlaceCarServices(carServicesInfo);
            });
        }

        [RelayCommand]
        private async Task ShowMyLocation()
        {
            await ExecuteSaveAsync(async () =>
            {
                if (MapService == null)
                    throw new InvalidOperationException("Один из сервисов не был загружен");

                await _apiService.CheckInternetConnection();

                await MapService.ExecuteGettingUserLocation(SearchRadius);
            });
        }

        [RelayCommand]
        private async Task SearchByQuery()
        {
            await ExecuteSaveAsync(async () =>
            {
                if (MapService == null)
                    throw new InvalidOperationException("Один из сервисов не был загружен");

                if (string.IsNullOrWhiteSpace(SearchQuery))
                    throw new InvalidOperationException("Строка запроса пустая!");

                if (MaxResults <= 0)
                    throw new InvalidOperationException("Максимальное количество мастерских, должно быть больше 0!");

                if (SearchRadius <= 0)
                    throw new InvalidOperationException("Радиус поиска, должен быть больше 0!");

                List<CarServiceInfo> carServicesInfo = new List<CarServiceInfo>();

                int maximumPages = MaxResults / _maxRecordsReturndForOneTime;
                int remainder = MaxResults % _maxRecordsReturndForOneTime;
                int currentPage = 0;

                if (remainder > 0)
                    maximumPages++;

                while (currentPage <= maximumPages)
                {
                    if (remainder > 0 && currentPage == maximumPages)
                    {
                        List<CarServiceInfo> lastList = await _apiService.GISGet(SearchQuery, currentPage, SearchRadius);
                        carServicesInfo.AddRange(lastList.Take(remainder));
                        break;
                    }

                    carServicesInfo.AddRange(await _apiService.GISGet(SearchQuery, currentPage, SearchRadius));
                    currentPage++;
                }

                if (carServicesInfo.Any())
                    DialogManager.ShowDialog<SuccessDialogViewModel>(new WarningDialogModel()
                    {
                        Title = "Успех!",
                        WarningContent = $"Количество найденых сервисов: {carServicesInfo.Count}",
                        ButtonContent = "Ок"
                    });
                else
                    DialogManager.ShowDialog<ErrorDialogViewModel>(new WarningDialogModel()
                    {
                        Title = "Ничего не найдено!",
                        WarningContent = $"Не было найдено ни одного сервиса по близости.",
                        ButtonContent = "Ок"
                    });

                await MapService.PlaceCarServices(carServicesInfo);
            });
        }

        [RelayCommand]
        private async Task ClearMap()
        {
            await ExecuteSaveAsync(async () =>
            {
                if (MapService == null)
                    throw new InvalidOperationException("Один из сервисов не был загружен");

                await MapService.ClearAllMap();
            });
        }

        public void SetUserMarker(string coordinate)
        {
            ExecuteSave(() =>
            {
                if (MapService == null)
                    throw new InvalidOperationException("Один из сервисов не был загружен");

                _userCoordinate = JsonSerializer.Deserialize<Point>(coordinate)
                    ?? throw new InvalidOperationException("Не удалось десериализовать данные");

                MapService.ShowUserLocation(_userCoordinate.Lat, _userCoordinate.Lon, SearchRadius);
            });
        }

        async partial void OnSearchRadiusChanged(int value)
        {
            await ExecuteSaveAsync(async () =>
            {
                if (value > 0)
                {
                    if (MapService == null)
                        throw new InvalidOperationException("Один из сервисов не был загружен");

                    await MapService.DrawRaiusCircle(_userCoordinate.Lat, _userCoordinate.Lon, value);
                }
            });
        }

        [RelayCommand]
        private async Task BackToHomePage() =>
            await NavigationService.NavigateTo<MenuViewModel>();

        public bool CheckFileExistence()
        {
            ExecuteSave(() =>
            {
                string directoryFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _webFilesDirectoryName);

                if (!Directory.Exists(directoryFullPath))
                    throw new DirectoryNotFoundException($"Папки: {_webFilesDirectoryName} не существует.");

                if (!File.Exists(Path.Combine(directoryFullPath, "WebView.html")))
                    throw new FileNotFoundException($"Файла: WebView.html не существует.");

                if (!File.Exists(Path.Combine(directoryFullPath, "WebView.css")))
                    throw new FileNotFoundException($"Файла: WebView.css не существует.");

                if (!File.Exists(Path.Combine(directoryFullPath, "WebView.js")))
                    throw new FileNotFoundException($"Файла: WebView.js не существует.");
            });

            return true;
        }
    }
}
