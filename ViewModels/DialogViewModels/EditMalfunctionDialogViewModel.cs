using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GearFix.Interfaces;
using GearFix.Models.AppModels;

namespace GearFix.ViewModels.DialogViewModels
{
    public partial class EditMalfunctionDialogViewModel : BaseDialogViewModel
    {
        private EditMalfunctionDialogModel _model;

        [ObservableProperty]
        private CarModel _selectedCar;

        [ObservableProperty]
        private string _malfunctionTitle = string.Empty;
        [ObservableProperty]
        private string _description = string.Empty;
        [ObservableProperty]
        private DateTime _date = DateTime.Now;
        [ObservableProperty]
        private int _danger = 1;

        public EditMalfunctionDialogViewModel(
            IDialogManager dialogManager,
            EditMalfunctionDialogModel? model) : base(dialogManager, model)
        {
            if (model == null)
                throw new InvalidOperationException("Вы не передали модель в диалоговое окно!");

            if (model.SelectedCar == null)
                throw new InvalidOperationException("Вы не выбрали машину, у которой обнаружилась ошибка!");

            _model = model;
            SelectedCar = _model.SelectedCar;

            if (_model.SelectedMalfunction != null)
            {
                Title = _model.Title;
                ButtonContent = _model.ButtonContent;

                MalfunctionTitle = _model.SelectedMalfunction.Title;
                Description = _model.SelectedMalfunction.Description;
                Date = new DateTime(_model.SelectedMalfunction.Date, new TimeOnly());

                if (_model.SelectedMalfunction.Danger <= 0)
                    Danger = 1;
                else
                    Danger = _model.SelectedMalfunction.Danger;
            }
        }

        [RelayCommand]
        private void Confirm()
        {
            if (_model == null || _model.SelectedCar == null)
            {
                ErrorMessage = "Модель или выбранная машина пусты!";
                return;
            }

            if (!CanConfirm())
                return;

            ErrorMessage = string.Empty;

            _model.SelectedMalfunction ??= new();

            _model.SelectedMalfunction.Title = MalfunctionTitle;
            _model.SelectedMalfunction.Description = Description;
            _model.SelectedMalfunction.Danger = Danger;
            _model.SelectedMalfunction.Date = new DateOnly(Date.Year, Date.Month, Date.Day);

            _model.SelectedCar.Malfunctions.Add(_model.SelectedMalfunction);

            DialogManager.CloseDialog(this, true);
        }

        private bool CanConfirm()
        {
            if (string.IsNullOrWhiteSpace(MalfunctionTitle))
            {
                ErrorMessage = "Вы не заполнили поле с названием ошибки!";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                ErrorMessage = "Информация об ошибке пуста! Пожалуйста, укажить хотябы какую-то информацию!";
                return false;
            }

            if (Danger < 1 || Danger > 100)
            {
                ErrorMessage = "Опасность не может выходить за диапозон значений от 1 до 100";
                return false;
            }


            if (DateTime.Compare(Date, DateTime.Now) > 0)
            {
                ErrorMessage = "Ошибка не может произойти в будущем! Исправьте дату!";
                return false;
            }

            return true;
        }

        [RelayCommand]
        private void Cancel() =>
            DialogManager.CloseDialog(this, false);
    }
}
