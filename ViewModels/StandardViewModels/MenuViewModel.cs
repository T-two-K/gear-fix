using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GearFix.Interfaces;
using GearFix.Models.AppModels;
using GearFix.ViewModels.DialogViewModels;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;

namespace GearFix.ViewModels
{
    public partial class MenuViewModel : BaseViewModel
    {
        private JsonDataModel _loadedData { get; set; }
        public ObservableCollection<CarModel> Cars { get; set; }

        [ObservableProperty]
        private bool _isEmpty = true;

        [ObservableProperty]
        private CarModel? _selectedCar;

        public MenuViewModel(
            IDialogManager dialogManager,
            IManageDataService manageDataService,
            INavigationService navigationService) : base(dialogManager, manageDataService, navigationService)
        {
            _loadedData = new JsonDataModel();
            Cars = new ObservableCollection<CarModel>();
        }

        protected override async Task LoadDataAsync()
        {
            _loadedData = await ManageDataService.LoadDataAsync();
            Cars = [.. _loadedData.Cars];
            Cars.CollectionChanged += OnCarsChanged;
            if (Cars.Any())
                IsEmpty = false;
        }

        public void CheckApiKey()
        {
            if (_loadedData.Keys == null || _loadedData.Keys.Count == 0)
            {
                DialogManager.ShowDialog<AiApiKeyDialogViewModel>(new BaseDialogModel());
            }
        }

        private async void OnCarsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            await ExecuteSaveAsync(async () =>
            {
                await ManageDataService.SaveDataAsync(Cars.ToList(), _loadedData.Keys);
            });
        }

        [RelayCommand]
        private void AddCar()
        {
            ExecuteSave(() =>
            {
                CarModel newCar = new CarModel();

                EditCarDialogModel dialogResult = (EditCarDialogModel)DialogManager.ShowDialog<EditCarDialogViewModel>(
                    new EditCarDialogModel()
                    {
                        Title = "Добавить машину",
                        ButtonContent = "Добавить",
                        SelectedCar = newCar
                    });

                if (dialogResult.DialogResult)
                {
                    if (!string.IsNullOrWhiteSpace(newCar.ImagePath) && Path.IsPathFullyQualified(newCar.ImagePath))
                        newCar.ImagePath = ManageDataService.ChangeImagePath(newCar.ImagePath);

                    Cars.Add(newCar);
                    IsEmpty = false;
                }
            });
        }

        [RelayCommand]
        private async Task EditCar(CarModel editingCar)
        {
            await ExecuteSaveAsync(async () =>
            {
                int index = Cars.IndexOf(editingCar);
                string? oldImagePath = editingCar.ImagePath;

                EditCarDialogModel dialogResult = (EditCarDialogModel)DialogManager.ShowDialog<EditCarDialogViewModel>(new EditCarDialogModel
                {
                    Title = "Изменить машину",
                    ButtonContent = "Изменить",
                    SelectedCar = editingCar
                });

                if (dialogResult.DialogResult)
                {
                    if (oldImagePath != editingCar.ImagePath && !string.IsNullOrWhiteSpace(editingCar.ImagePath))
                    {
                        editingCar.ImagePath = ManageDataService.ChangeImagePath(editingCar.ImagePath);

                        if (oldImagePath != null && Path.Exists(oldImagePath) && oldImagePath != editingCar.ImagePath)
                            ManageDataService.DeleteOldImagePath(oldImagePath);
                    }

                    Cars.RemoveAt(index);
                    await Task.Delay(200);
                    Cars.Insert(index, editingCar);
                }
            });
        }

        [RelayCommand]
        private void DeleteCar(CarModel deletedCar)
        {
            WarningDialogModel dialogResult = (WarningDialogModel)DialogManager.ShowDialog<WarningDialogViewModel>(new WarningDialogModel()
            {
                Title = "Внимание!",
                ButtonContent = "Подтвердить",
                WarningContent = $"Вы точно хотите удалить {deletedCar.Model} - {deletedCar.Make} ({deletedCar.Year})?"
            });

            if (dialogResult.DialogResult)
            {
                if (deletedCar.ImagePath != null)
                    if (!Cars.Where(c => c.ImagePath == deletedCar.ImagePath && c != deletedCar).Any())
                    {
                        ManageDataService.DeleteOldImagePath(deletedCar.ImagePath);
                    }

                Cars.Remove(deletedCar);

                if (Cars.Count <= 0)
                    IsEmpty = true;
            }
        }

        partial void OnSelectedCarChanged(CarModel? value)
        {
            if(value != null)
            {
                InfoCar(value);
            }
        }

        private void InfoCar(CarModel infoCar)
        {
            DialogManager.ShowDialog<DisplayCarInfoDialogViewModel>(new EditCarDialogModel()
            {
                SelectedCar = infoCar
            });
        }

        [RelayCommand]
        private async Task NavigateDiagnostics() =>
            await NavigationService.NavigateTo<AISpecialistViewModel>();

        [RelayCommand]
        private async Task NavigateToMap() =>
            await NavigationService.NavigateTo<MapViewModel>();

        [RelayCommand]
        private async Task NavigateSettings() =>
            await NavigationService.NavigateTo<SettingsViewModel>();

    }
}
