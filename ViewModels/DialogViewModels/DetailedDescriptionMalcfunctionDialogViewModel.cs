using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GearFix.DTO;
using GearFix.Interfaces;
using GearFix.Models.ApiModels.GisModels;
using GearFix.Models.AppModels;
using System.Collections.ObjectModel;
using System.Windows;

namespace GearFix.ViewModels.DialogViewModels
{
    public partial class DetailedDescriptionMalcfunctionDialogViewModel : BaseDialogViewModel
    {
        private IApiService _apiService;

        [ObservableProperty]
        private string _nearestCity = string.Empty;
        private string _rubric = string.Empty;

        [ObservableProperty]
        private string _malfunctionDescription = string.Empty;
        [ObservableProperty]
        private string _explanation = string.Empty;
        [ObservableProperty]
        private int _probability;
        [ObservableProperty]
        private int _danger;

        public ObservableCollection<CarServicesDto> CarServices { get; set; } = new();

        public DetailedDescriptionMalcfunctionDialogViewModel(
            IDialogManager dialogManager,
            IApiService apiService,
            DetailedDescriptionMalcfunctionDialogModel? model) : base(dialogManager, model)
        {
            if (model == null || model.Malfunction == null)
                throw new InvalidOperationException("Вы не выбрали ошибку!");

            _apiService = apiService;


            Title = model.Malfunction.Title;
            MalfunctionDescription = model.Malfunction.MalfunctionDescription;
            Explanation = model.Malfunction.Explanation;
            Probability = model.Malfunction.Probability;
            Danger = model.Malfunction.Danger;
            _rubric = model.Malfunction.Rubric;
        }

        [RelayCommand]
        private async Task FindCarServices()
        {
            await ExecuteSaveAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(NearestCity))
                    throw new InvalidOperationException("Вы не ввели ближайший город!");

                string query = $"{NearestCity}, {_rubric}";

                List<CarServiceInfo> carServicesInfo = await _apiService.GISGet(query, 1);

                if (carServicesInfo != null && carServicesInfo.Any())
                {
                    CarServices.Clear();

                    foreach (var item in carServicesInfo.Select(csi => new CarServicesDto()
                    {
                        Name = csi.Name,
                        Address = csi.AddressName,
                        Rubrics = csi.Rubrics.Select(r => r.Name).ToList()
                    }).Take(5).ToList())
                        CarServices.Add(item);
                }

                if (CarServices == null)
                    throw new InvalidOperationException("Сервисы не найдены!");
            });
        }

        [RelayCommand]
        private void Copy(CarServicesDto carService)
        {
            if (!string.IsNullOrWhiteSpace(NearestCity))
                Clipboard.SetText($"{NearestCity}, {carService.Name}");

            DialogManager.ShowDialog<SuccessDialogViewModel>(
                new WarningDialogModel()
                {
                    Title = "Успех!",
                    ButtonContent = "OK",
                    WarningContent = "Копирование прошло успешно! Чтобы найти заведение на карте вставьте текст в поисковую строку."
                });
        }

        [RelayCommand]
        private void Back() =>
            DialogManager.CloseDialog(this, true);
    }
}
