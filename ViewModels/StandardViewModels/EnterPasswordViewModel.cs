using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GearFix.Interfaces;
using GearFix.Models.AppModels;
using GearFix.ViewModels.DialogViewModels;

namespace GearFix.ViewModels
{
    public partial class EnterPasswordViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string _textBoxPassword = string.Empty;

        public EnterPasswordViewModel(
            IDialogManager dialogManager,
            IManageDataService manageDataService,
            INavigationService navigationService) : base( dialogManager, manageDataService, navigationService)
        {

        }

        [RelayCommand]
        private async Task Login()
        {
            await ExecuteSaveAsync(async () =>
            {
                if(await ManageDataService.TryLoadDataAsync(TextBoxPassword))
                {
                    AiApiKeyDialogModel? apiKeyDialogModel = null;
                    if (!await ManageDataService.CheckApiKeys())
                    {
                        apiKeyDialogModel = (AiApiKeyDialogModel)DialogManager.ShowDialog<AiApiKeyDialogViewModel>(new AiApiKeyDialogModel());

                        if (apiKeyDialogModel.DialogResult)
                            await ManageDataService.SaveApiKeyAsync(apiKeyDialogModel.ApiKey);
                    }

                    await NavigationService.NavigateTo<MenuViewModel>();
                }
            });
        }
    }
}
