using GearFix.Interfaces;
using GearFix.ViewModels;

namespace GearFix.Services
{
    public class NavigationService : INavigationService
    {
        public BaseViewModel CurrentViewModel { get; set; } = null!;

        private IServiceProvider _provider;

        public event Action? UpdatedViewModel;

        public NavigationService(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task NavigateTo<TViewModel>() where TViewModel : BaseViewModel
        {
            CurrentViewModel = await BaseViewModel.Initialize<TViewModel>(_provider);
            UpdatedViewModel?.Invoke();
        }
    }
}
