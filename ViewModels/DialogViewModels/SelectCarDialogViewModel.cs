using CommunityToolkit.Mvvm.Input;
using GearFix.Interfaces;
using GearFix.Models.AppModels;

namespace GearFix.ViewModels.DialogViewModels
{
    public partial class SelectCarDialogViewModel : BaseDialogViewModel
    {
        private SelectCarDialogModel _model;
        public List<CarModel> Cars { get; set; }

        public SelectCarDialogViewModel(
            IDialogManager dialogManager,
            SelectCarDialogModel? model) : base(dialogManager, model)
        {
            if (model == null)
                throw new InvalidOperationException("Вы не передали параметры в диалоговое окно!");

            _model = model;
            Cars = model.Cars;
        }

        [RelayCommand]
        private void Cancel() =>
            DialogManager.CloseDialog(this, false);

        [RelayCommand]
        private void SelectCar(CarModel selectedCar)
        {
            _model.CurrentCar = selectedCar;
            DialogManager.CloseDialog(this, true);
        }
    }
}
