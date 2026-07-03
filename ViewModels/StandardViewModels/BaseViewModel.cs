using CommunityToolkit.Mvvm.ComponentModel;
using GearFix.Interfaces;
using GearFix.Models.AppModels;
using GearFix.ViewModels.DialogViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace GearFix.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        protected IManageDataService ManageDataService { get; set; }
        protected INavigationService NavigationService { get; set; }
        protected IDialogManager DialogManager { get; set; }

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        public BaseViewModel(
            IDialogManager dialogManager,
            IManageDataService manageDataService,
            INavigationService navigationService)
        {
            ManageDataService = manageDataService;
            NavigationService = navigationService;
            DialogManager = dialogManager;
        }

        static public async Task<TViewModel> Initialize<TViewModel>(IServiceProvider provider)
            where TViewModel : BaseViewModel
        {
            return await Task.Run(async () =>
            {
                var vm = ActivatorUtilities.CreateInstance<TViewModel>(provider);
                await vm.LoadDataAsync();
                return vm;
            });
        }

        protected virtual Task LoadDataAsync() => Task.CompletedTask;
        
        protected void ExecuteSave(Action executed)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                executed.Invoke();
            }
            catch (TaskCanceledException)
            {
                DialogManager.ShowDialog<ErrorDialogViewModel>(new WarningDialogModel
                    { WarningContent = "Нестабильное подключение к интернету!" });
            }
            catch (InvalidOperationException ex)
            {
                DialogManager.ShowDialog<ErrorDialogViewModel>(new WarningDialogModel {WarningContent = ex.Message});
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Необработанное исключение! {ex.Message}";
                DialogManager.ShowDialog<ErrorDialogViewModel>(new WarningDialogModel { WarningContent = ErrorMessage });
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected async Task ExecuteSaveAsync(Func<Task> executed)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                await executed.Invoke();
            }
            catch (TaskCanceledException)
            {
                DialogManager.ShowDialog<ErrorDialogViewModel>(new WarningDialogModel
                    { WarningContent = "Нестабильное подключение к интернету!" });
            }
            catch (InvalidOperationException ex)
            {
                DialogManager.ShowDialog<ErrorDialogViewModel>(new WarningDialogModel { WarningContent = ex.Message });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Необработанное исключение! {ex.Message}";
                DialogManager.ShowDialog<ErrorDialogViewModel>(new WarningDialogModel { WarningContent = ErrorMessage });
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
