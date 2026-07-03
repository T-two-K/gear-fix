using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GearFix.Models.AppModels;
using GearFix.Interfaces;
using GearFix.ViewModels.DialogViewModels;

namespace GearFix.ViewModels
{
    public partial class CreatePasswordViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string _textBoxPassword = string.Empty;
        [ObservableProperty]
        private string _textBoxRepeatPassword = string.Empty;

        public CreatePasswordViewModel(
            IDialogManager dialogManager,
            INavigationService navigationService,
            IManageDataService manageDataService) : base(dialogManager, manageDataService, navigationService)
        {

        }

        //Придумай как передавать данные из этого окна в другое, было предложение всё хранить в MainWindow
        //(предложение мне не понравилось, я придумал лучше).
        [RelayCommand]
        private async Task CreatePassword()
        {
            if (CanCreatePassword())
                await ExecuteSaveAsync(async () =>
                {
                    await ManageDataService.CreatePassword(TextBoxPassword);

                    AiApiKeyDialogModel? apiKeyDialogModel = null;

                    apiKeyDialogModel = (AiApiKeyDialogModel)DialogManager.ShowDialog<AiApiKeyDialogViewModel>(new AiApiKeyDialogModel());

                    if (apiKeyDialogModel.DialogResult)
                        await ManageDataService.SaveApiKeyAsync(apiKeyDialogModel.ApiKey);

                    await NavigationService.NavigateTo<MenuViewModel>();
                });
        }

        private bool CanCreatePassword()
        {
            if (string.IsNullOrWhiteSpace(TextBoxPassword) ||
               string.IsNullOrWhiteSpace(TextBoxRepeatPassword))
            {
                ErrorMessage = "Заполните поля!";
                return false;
            }

            if (TextBoxPassword != TextBoxRepeatPassword)
            {
                ErrorMessage = "Пароли не совпадают!";
                return false;
            }

            return true;
        }
    }
}
