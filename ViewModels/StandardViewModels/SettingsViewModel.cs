using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GearFix.Interfaces;
using GearFix.Models.AppModels;
using GearFix.ViewModels.DialogViewModels;

namespace GearFix.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        private const int geminiKeyIndex = 0;

        [ObservableProperty]
        private string _geminiApiKey = string.Empty;

        public SettingsViewModel(IDialogManager dialogManager,
            IManageDataService manageDataService,
            INavigationService navigationService)
            : base(dialogManager, manageDataService, navigationService)
        {

        }

        protected override async Task LoadDataAsync()
        {
            JsonDataModel jsonData = await ManageDataService.LoadDataAsync();

            if (jsonData.Keys == null)
                return;

            if (jsonData.Keys.Count != 0)
                GeminiApiKey = jsonData.Keys[geminiKeyIndex].Substring(0, 10).Trim() + "...";
        }

        [RelayCommand]
        private void BackHomePage() =>
            NavigationService.NavigateTo<MenuViewModel>();

        [RelayCommand]
        private async Task EditGeminiKey()
        {
            await ExecuteSaveAsync(async () =>
            {
                JsonDataModel jsonData = await ManageDataService.LoadDataAsync();

                string currentApiKey = string.Empty;

                if (jsonData.Keys.Count == 0)
                    currentApiKey = string.Empty;
                else currentApiKey = jsonData.Keys[geminiKeyIndex];

                AiApiKeyDialogModel model = new()
                {
                    ApiKey = currentApiKey
                };

                if (DialogManager.ShowDialog<AiApiKeyDialogViewModel>(model).DialogResult)
                {
                    if (jsonData.Cars == null)
                        jsonData.Cars = new List<CarModel>();

                    jsonData.Keys[geminiKeyIndex] = currentApiKey;

                    await ManageDataService.SaveDataAsync(jsonData.Cars, jsonData.Keys);

                    GeminiApiKey = jsonData.Keys[geminiKeyIndex].Substring(0, 10).Trim() + "...";
                }
            });
        }

        [RelayCommand]
        private async Task DeleteGeminiKey()
        {
            await ExecuteSaveAsync(async () =>
            {
                WarningDialogModel model = new()
                {
                    Title = "Внимание!",
                    ButtonContent = "Подтвердить",
                    WarningContent = "Вы точно хотите удалить свой API ключ?"
                };

                if (DialogManager.ShowDialog<WarningDialogViewModel>(model).DialogResult)
                {
                    JsonDataModel jsonData = await ManageDataService.LoadDataAsync();

                    jsonData.Keys[geminiKeyIndex] = string.Empty;

                    await ManageDataService.SaveDataAsync(jsonData.Cars, jsonData.Keys);

                    GeminiApiKey = string.Empty;
                }
            });
        }

        [RelayCommand]
        private void ChangePassword() =>
            DialogManager.ShowDialog<ChangePasswordDialogViewModel>(new BaseDialogModel());

        [RelayCommand]
        private void DeleteAllData()
        {
            if(DialogManager.ShowDialog<WarningDialogViewModel>(new WarningDialogModel() 
            { 
                Title = "Внимание!",
                ButtonContent = "Подтвердить",
                WarningContent = "Вы точно хотите удалить все данные? После нажатия на кнопку \"Подтвердить\"" +
                " абсолютно все данные приложения будут удалены с устройства, а приложение завершит свою работу, продолжить?"
            }).DialogResult)
            {
                ManageDataService.DeleteAllDataAndCloseApp();
            }
        }
    }
}
