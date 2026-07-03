using CommunityToolkit.Mvvm.ComponentModel;
using GearFix.Interfaces;

namespace GearFix.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        [ObservableProperty]
        private BaseViewModel? _currentViewModel;

        public MainViewModel(
            IDialogManager dialogManager,
            IManageDataService mainService,
            INavigationService navigationService) : base( dialogManager, mainService, navigationService) 
        {
            NavigationService.UpdatedViewModel += UpdateViewModel;
            Initialize();
        }

        public void Initialize()
        {
            if (ManageDataService.CheckFileExistence())
                NavigationService.NavigateTo<EnterPasswordViewModel>();
            else
                NavigationService.NavigateTo<CreatePasswordViewModel>();
        }

        public void UpdateViewModel()
        {
            CurrentViewModel = NavigationService.CurrentViewModel;
        }
    }
}
