using CommunityToolkit.Mvvm.Input;
using GearFix.Interfaces;
using GearFix.Models.AppModels;

namespace GearFix.ViewModels.DialogViewModels
{
    public partial class DisplayCarInfoDialogViewModel : BaseDialogViewModel
    {
        public CarModel Car { get; set; }

        public DisplayCarInfoDialogViewModel(IDialogManager dialogManager,
            EditCarDialogModel? model) : base(dialogManager, model)
        {
            if (model == null || model.SelectedCar == null)
                throw new InvalidOperationException("Вы не выбрали машину!");

            Car = model.SelectedCar;

            Car.Malfunctions = Car.Malfunctions.OrderByDescending(m => m.Danger).ToList();
        }

        [RelayCommand]
        private void Close() =>
            DialogManager.CloseDialog(this, false);
    }
}
