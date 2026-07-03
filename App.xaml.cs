using GearFix.Interfaces;
using GearFix.Services;
using GearFix.ViewModels;
using GearFix.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace GearFix
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceBuilder = new ServiceCollection();

            serviceBuilder.AddSingleton<IFileService, FileService>();
            serviceBuilder.AddSingleton<IHashService, HashService>();
            serviceBuilder.AddSingleton<IDialogManager, DialogManagerService>();
            serviceBuilder.AddSingleton<IEncryptionService, EncryptionService>();
            serviceBuilder.AddSingleton<INavigationService,  NavigationService>();
            serviceBuilder.AddSingleton<IManageDataService, ManageDataService>();
            serviceBuilder.AddSingleton<IApiService, ApiService>();

            serviceBuilder.AddSingleton<MainWindow>();
            serviceBuilder.AddTransient<MainViewModel>();

            serviceBuilder.AddTransient<BaseViewModel>();

            _serviceProvider = serviceBuilder.BuildServiceProvider();

            var fileService = _serviceProvider.GetRequiredService<IFileService>();
            fileService.CreateNesesaryDirectory();

            var window = _serviceProvider.GetRequiredService<MainWindow>();
            window.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();

            window.Show();
        }
    }
}