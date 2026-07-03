using CommunityToolkit.Mvvm.ComponentModel;
using GearFix.Interfaces;
using GearFix.Models.AppModels;

namespace GearFix.ViewModels.DialogViewModels
{
    public partial class BaseDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _title = string.Empty;
        [ObservableProperty]
        private string _buttonContent = string.Empty;

        [ObservableProperty]
        private string? _errorMessage;

        protected IDialogManager DialogManager { get; set; }

        public BaseDialogViewModel(IDialogManager dialogManager, BaseDialogModel? model)
        {
            if (model != null)
            {
                Title = model.Title;
                ButtonContent = model.ButtonContent;
            }

            DialogManager = dialogManager;
        }

        protected async Task ExecuteSaveAsync(Func<Task> executed)
        {
            try
            {
                ErrorMessage = string.Empty;
                await executed.Invoke();
            }
            catch (InvalidOperationException ex)
            {
                DialogManager.ShowDialog<ErrorDialogViewModel>(new WarningDialogModel { WarningContent = ex.Message });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Не обработанное исключение! {ex.Message}";
                DialogManager.ShowDialog<ErrorDialogViewModel>(new WarningDialogModel { WarningContent = ErrorMessage });
            }
        }

        protected void ExecuteSave(Action executed)
        {
            try
            {
                ErrorMessage = string.Empty;
                executed.Invoke();
            }
            catch (InvalidOperationException ex)
            {
                DialogManager.ShowDialog<ErrorDialogViewModel>(new WarningDialogModel { WarningContent = ex.Message });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Не обработанное исключение! {ex.Message}";
                DialogManager.ShowDialog<ErrorDialogViewModel>(new WarningDialogModel { WarningContent = ErrorMessage });
            }
        }
    }
}
