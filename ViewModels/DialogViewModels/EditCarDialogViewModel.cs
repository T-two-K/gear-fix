using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GearFix.Interfaces;
using GearFix.Models.AppModels;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace GearFix.ViewModels.DialogViewModels
{
    public partial class EditCarDialogViewModel : BaseDialogViewModel
    {
        private const int _firstCarWasCreated = 1885;
        private const int _standardVinLength = 11;

        public List<string> EngineTypes { get; set; } = new List<string>()
        {
            "Бензин", "Дизель", "Гибрид", "Електрический", "LPG"
        };

        [ObservableProperty]
        private CarModel _car;

        private EditCarDialogModel _model;

        [ObservableProperty]
        private string _textBoxModel = string.Empty;
        [ObservableProperty]
        private string _textBoxMake = string.Empty;
        [ObservableProperty]
        private string _textBoxYear = string.Empty;
        [ObservableProperty]
        private string _textBoxMileage = string.Empty;
        [ObservableProperty]
        private string _textBoxVin = string.Empty;
        [ObservableProperty]
        private string? _selectedEngineType;
        [ObservableProperty]
        private string? _textBoxAdditionalInfo;
        [ObservableProperty]
        private string? _imagePath;

        public ObservableCollection<MalfunctionModel> Malfunctions { get; set; }

        public EditCarDialogViewModel(IDialogManager dialogManager, EditCarDialogModel? model)
            : base(dialogManager, model)
        {
            if (model == null)
                throw new InvalidOperationException("Вы не выбрали машину!");

            _model = model;

            if (model.SelectedCar == null)
            {
                Car = new CarModel();
                Malfunctions = new ObservableCollection<MalfunctionModel>();
            }
            else
            {
                Car = model.SelectedCar;
                Malfunctions = [.. model.SelectedCar.Malfunctions];
                FillForm();
            }
        }

        private void FillForm()
        {
            if (_model.SelectedCar != null)
            {
                TextBoxModel = _model.SelectedCar.Model;
                TextBoxMake = _model.SelectedCar.Make;
                TextBoxYear = _model.SelectedCar.Year.ToString();
                TextBoxMileage = _model.SelectedCar.Mileage.ToString();
                TextBoxVin = _model.SelectedCar.Vin;
                SelectedEngineType = _model.SelectedCar.EngineType;
                TextBoxAdditionalInfo = _model.SelectedCar.AdditionalInfo;
                ImagePath = _model.SelectedCar.ImagePath;
            }
        }

        [RelayCommand]
        private void ConfirmOperation()
        {
            if (!CheckValid())
                return;

            if (_model.SelectedCar == null)
                _model.SelectedCar = new();

            _model.SelectedCar.Make = TextBoxMake;
            _model.SelectedCar.Model = TextBoxModel;
            _model.SelectedCar.Year = int.Parse(TextBoxYear);
            _model.SelectedCar.Mileage = int.Parse(TextBoxMileage ?? "0");
            _model.SelectedCar.Vin = TextBoxVin;
            _model.SelectedCar.EngineType = SelectedEngineType;
            _model.SelectedCar.AdditionalInfo = TextBoxAdditionalInfo;
            _model.SelectedCar.ImagePath = ImagePath;
            _model.SelectedCar.Malfunctions = Malfunctions.ToList();

            DialogManager.CloseDialog(this, true);
        }

        [RelayCommand]
        private void CancelOperation() =>
            DialogManager.CloseDialog(this, false);

        [RelayCommand]
        private void PickImage()
        {
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Filter =
                    "(.png)|*.png" +
                    "|(.jpg)|*.jpg" +
                    "|(.webp)|*.webp" +
                    "|(.gif)|*.gif"
            };

            fileDialog.ShowDialog();
            ImagePath = fileDialog.FileName;
        }

        [RelayCommand]
        private void AddMalfunction()
        {
            ExecuteSave(() =>
            {
                if (_model == null || _model.SelectedCar == null)
                    throw new InvalidOperationException("Передоваемая модель или автомобиль были пустыми.");

                EditMalfunctionDialogModel model = new()
                {
                    SelectedCar = _model.SelectedCar,
                    SelectedMalfunction = null,
                    Title = "Добавить новую неисправность",
                    ButtonContent = "Добавить"
                };

                if (DialogManager.ShowDialog<EditMalfunctionDialogViewModel>(model).DialogResult
                    && model.SelectedMalfunction != null)
                {
                    _model.SelectedCar.Malfunctions.Add(model.SelectedMalfunction);
                    Malfunctions.Add(model.SelectedMalfunction);
                }
            });
        }

        [RelayCommand]
        private void RemoveMalfunction(MalfunctionModel deletingMalfunction)
        {
            ExecuteSave(() =>
            {
                if (_model == null || _model.SelectedCar == null)
                    throw new InvalidOperationException("Передоваемая модель или автомобиль были пустыми.");

                if (DialogManager.ShowDialog<WarningDialogViewModel>(new WarningDialogModel()
                {
                    Title = "Внимание!",
                    WarningContent = $"Вы точно хотите удалить ошибку \"{deletingMalfunction.Title}\"?",
                    ButtonContent = "Подтвердить"
                }).DialogResult)
                {
                    _model.SelectedCar.Malfunctions.Remove(deletingMalfunction);
                    Malfunctions.Remove(deletingMalfunction);
                }
            });
        }

        [RelayCommand]
        private void EditMalfunction(MalfunctionModel editingMalfunction)
        {
            EditMalfunctionDialogModel model = new()
            {
                SelectedCar = _model.SelectedCar,
                SelectedMalfunction = editingMalfunction,
                Title = "Изменить информацию об ошибке",
                ButtonContent = "Изменить"
            };

            DialogManager.ShowDialog<EditMalfunctionDialogViewModel>(model);
        }

        private bool CheckValid()
        {
            if (string.IsNullOrEmpty(TextBoxModel))
            {
                ErrorMessage = "Вы не заполнили поле \"Марка\"";
                return false;
            }

            if (string.IsNullOrEmpty(TextBoxMake))
            {
                ErrorMessage = "Вы не заполнили поле \"Модель\"";
                return false;
            }

            if (string.IsNullOrEmpty(TextBoxYear))
            {
                ErrorMessage = "Вы не заполнили поле \"Год\"";
                return false;
            }

            if (!int.TryParse(TextBoxYear, out int carYear) || carYear < _firstCarWasCreated)
            {
                ErrorMessage = "В поле \"Год\" невалидные данные! Машины с таким годом не может существовать!";
                return false;
            }

            if (!int.TryParse(TextBoxMileage, out int mileage) || mileage < 0)
            {
                ErrorMessage = "В поле \"Пробег\" невалидные данные!";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(TextBoxVin) && TextBoxVin.Length != _standardVinLength)
            {
                ErrorMessage = "VIN транспорта должен иметь 11 символов!";
                return false;
            }

            return true;
        }
    }
}