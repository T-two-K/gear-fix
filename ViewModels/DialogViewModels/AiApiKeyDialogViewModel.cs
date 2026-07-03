using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GearFix.Interfaces;
using GearFix.Models.AppModels;
using System.Diagnostics;

namespace GearFix.ViewModels.DialogViewModels
{
    public partial class AiApiKeyDialogViewModel : BaseDialogViewModel
    {
        private IApiService _apiService;

        private AiApiKeyDialogModel _model;

        [ObservableProperty]
        private string _apiKey = string.Empty;

        public AiApiKeyDialogViewModel(
            IDialogManager dialogManager,
            IApiService apiService,
            AiApiKeyDialogModel model) : base(dialogManager, model)
        {
            _model = model;
            _apiService = apiService;

            if(!string.IsNullOrWhiteSpace(model.ApiKey))
                ApiKey = model.ApiKey;
        }

        [RelayCommand]
        private async Task Connect()
        {
            await ExecuteSaveAsync(async () =>
            {
                if (await _apiService.TryConnectToGeminiAsync(ApiKey))
                {
                    _model.ApiKey = ApiKey;
                    DialogManager.CloseDialog(this, true);
                }
            });
        }

        [RelayCommand]
        private async Task OpenApiKeyPage(Uri uri)
        {
            await ExecuteSaveAsync(async () =>
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = uri.AbsoluteUri,
                    UseShellExecute = true
                });
            });
        }
    }
}