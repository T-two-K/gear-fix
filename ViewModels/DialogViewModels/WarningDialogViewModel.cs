using CommunityToolkit.Mvvm.Input;
using GearFix.Interfaces;
using GearFix.Models.AppModels;

namespace GearFix.ViewModels.DialogViewModels
{
    public partial class WarningDialogViewModel : BaseDialogViewModel
    {
        public string WarningContent { get; set; } = string.Empty;

        public WarningDialogViewModel(IDialogManager dialogManager,
            WarningDialogModel? model) : base(dialogManager, model)
        {
            if (model == null)
                throw new InvalidOperationException("Вы не передали параметры в диалоговое окно!");

            WarningContent = model.WarningContent;
        }

        [RelayCommand]
        private void Confirm() =>
            DialogManager.CloseDialog(this, true);

        [RelayCommand]
        private void Cancel() =>
            DialogManager.CloseDialog(this, false);
    }
}
