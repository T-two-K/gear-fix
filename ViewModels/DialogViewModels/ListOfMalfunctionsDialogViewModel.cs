using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GearFix.Interfaces;
using GearFix.Models.ApiModels.AiModels;
using GearFix.Models.AppModels;
using System.Collections.ObjectModel;

namespace GearFix.ViewModels.DialogViewModels
{
    public partial class ListOfMalfunctionsDialogViewModel : BaseDialogViewModel
    {
        private ListOfMalfunctionsDialogModel _model { get; set; }

        public ObservableCollection<Malfunction> Malfunctions { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
        public Malfunction? _selectedMalfunction = null;

        //По хорошему необходимо реализовать ViewModel нашего окна диалога,
        //но перед этим разберись: сможешь ли ты из диалога открывать ещё один диалог (смогу)
        public ListOfMalfunctionsDialogViewModel(
            IDialogManager dialogManager,
            ListOfMalfunctionsDialogModel? model) : base(dialogManager, model)
        {
            if (model == null)
                throw new InvalidOperationException("Модель, передаваемая в диалоговое окно, не была инициализирована!");

            _model = model;
            Malfunctions = [.. _model.DiagnosInfo.Malfunctions.OrderByDescending(malf => malf.Probability)];
        }

        [RelayCommand(CanExecute = nameof(CanExecuteConfirm))]
        private void Confirm()
        {
            ExecuteSave(() =>
            {
                if (SelectedMalfunction == null)
                    throw new InvalidOperationException("Вы не выбрали неисправность.");

                _model.SelectedMalfunction = SelectedMalfunction;
                DialogManager.CloseDialog(this, true);
            });
        }

        [RelayCommand]
        private void MoreInfo(Malfunction currentMalfunction)
        {
            ExecuteSave(() =>
            {
                DetailedDescriptionMalcfunctionDialogModel model = new()
                {
                    Malfunction = currentMalfunction,
                    Title = currentMalfunction.Title,
                    ButtonContent = "Закрыть"
                };

                DialogManager.ShowDialog<DetailedDescriptionMalcfunctionDialogViewModel>(model);
            });
        }

        [RelayCommand]
        private void Cancel() =>
            DialogManager.CloseDialog(this, false);

        private bool CanExecuteConfirm()
            => SelectedMalfunction != null;
    }
}
