using GearFix.Interfaces;
using GearFix.Models.AppModels;
using GearFix.ViewModels.DialogViewModels;
using GearFix.Views.DialogViews;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace GearFix.Services
{
    public class DialogManagerService : IDialogManager
    {
        private Dictionary<Type, Type> _viewModelsWindows = new()
        {
            {typeof(EditCarDialogViewModel), typeof(EditCarDialog)},
            {typeof(WarningDialogViewModel), typeof(WarningDialog)},
            {typeof(SelectCarDialogViewModel), typeof(SelectCarDialogView)},
            {typeof(ErrorDialogViewModel), typeof(ErrorDialogView) },
            {typeof(AiApiKeyDialogViewModel), typeof(AiApiKeyDialogView)},
            {typeof(ChangePasswordDialogViewModel), typeof(ChangePasswordDialogView)},
            {typeof(SuccessDialogViewModel), typeof(SuccessDialogView)},
            {typeof(ListOfMalfunctionsDialogViewModel), typeof(ListOfMalfunctionsDialogView)},
            {typeof(DetailedDescriptionMalcfunctionDialogViewModel), typeof(DetailedDescriptionMalcfunctionDialogView)},
            {typeof(EditMalfunctionDialogViewModel), typeof(EditMalfunctionDialogView)},
            {typeof(DisplayCarInfoDialogViewModel), typeof(DisplayCarInfoDialogView)},
        };

        private Dictionary<BaseDialogViewModel, Window> _openedDialogWindows = new();

        private IServiceProvider _provider;

        public DialogManagerService(IServiceProvider provider)
        {
            _provider = provider;
        }

        public void CloseDialog<TViewModel>(TViewModel viewModel, bool result) where TViewModel : BaseDialogViewModel
        {
            if (!_openedDialogWindows.TryGetValue(viewModel, out Window? window) || window == null)
                throw new InvalidOperationException("Увы, но это диалоговое окно не открыто! (DialogManager)");

            window.DialogResult = result;
            _openedDialogWindows.Remove(viewModel);
            window.Close();
        }

        public BaseDialogModel ShowDialog<TViewModel>(BaseDialogModel model) where TViewModel : BaseDialogViewModel
        {
            if (!_viewModelsWindows.TryGetValue(typeof(TViewModel), out Type? windowType)
                || windowType == null)
            {
                throw new InvalidOperationException("Нет такой пары окна и viewModel! (DialogManager)");
            }

            var resultModel = model;
            var viewModel = ActivatorUtilities.CreateInstance<TViewModel>(_provider, resultModel);
            var window = (Window)ActivatorUtilities.CreateInstance(_provider, windowType);
            window.DataContext = viewModel;
            _openedDialogWindows.Add(viewModel, window);
            window.ShowDialog();

            resultModel.DialogResult = window.DialogResult ?? false;

            return resultModel;
        }
    }
}
