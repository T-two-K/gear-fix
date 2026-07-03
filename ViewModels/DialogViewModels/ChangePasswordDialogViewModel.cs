using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GearFix.Interfaces;
using GearFix.Models.AppModels;

namespace GearFix.ViewModels.DialogViewModels
{
    public partial class ChangePasswordDialogViewModel : BaseDialogViewModel
    {
        [ObservableProperty]
        private string _currentPassword = string.Empty;
        [ObservableProperty]
        private string _newPassword = string.Empty;
        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        private IManageDataService _mangeDataServise;

        public ChangePasswordDialogViewModel(
            IManageDataService manageDataService,
            IDialogManager dialogManager,
            BaseDialogModel? model) : base(dialogManager, model)
        {
            _mangeDataServise = manageDataService;
        }

        [RelayCommand]
        private async Task Confirm()
        {
            if (string.IsNullOrWhiteSpace(CurrentPassword) ||
                string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
                    throw new InvalidOperationException("Не все поля заполнены!");

            await ExecuteSaveAsync(async () =>
            {
                if (NewPassword != ConfirmPassword)
                    throw new InvalidOperationException("Пароли не совпадают. Введите новый пароль повторно!");

                if (await _mangeDataServise.ChangePasswordAsync(CurrentPassword, NewPassword))
                {
                    DialogManager.ShowDialog<SuccessDialogViewModel>(new WarningDialogModel()
                    {
                        ButtonContent = "Ок",
                        WarningContent = "Вы успешно сменили пароль!"
                    });

                    DialogManager.CloseDialog(this, true);
                }
                else
                    throw new InvalidOperationException("Не удалось сменить пароль, неизвестная ошибка.");

            });
        }

        [RelayCommand]
        private void Close() =>
            DialogManager.CloseDialog(this, false);
    }
}
