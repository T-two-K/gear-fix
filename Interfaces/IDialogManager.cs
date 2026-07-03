using GearFix.Models.AppModels;
using GearFix.ViewModels.DialogViewModels;

namespace GearFix.Interfaces
{
    public interface IDialogManager
    {
        public BaseDialogModel ShowDialog<TViewModel>(BaseDialogModel model) where TViewModel : BaseDialogViewModel;
        public void CloseDialog<TViewModel>(TViewModel viewModel, bool result) where TViewModel : BaseDialogViewModel;
    }
}
