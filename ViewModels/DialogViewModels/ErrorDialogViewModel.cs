using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GearFix.Interfaces;
using GearFix.Models.AppModels;

namespace GearFix.ViewModels.DialogViewModels
{
    public partial class ErrorDialogViewModel : BaseDialogViewModel
    {
        [ObservableProperty]
        private string _warningContent;

        public ErrorDialogViewModel(
            IDialogManager dialogManager,
            WarningDialogModel? model) : base(dialogManager, model)
        {
            if (model == null)
                throw new InvalidOperationException("Вы не передали параметры в диалоговое окно!");

            WarningContent = model.WarningContent;
        }

        [RelayCommand]
        private void Confirm() =>
            DialogManager.CloseDialog(this, true);
    }
}
