using GearFix.Services;
using GearFix.ViewModels;
using Microsoft.Web.WebView2.Core;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GearFix.Views
{
    public partial class MapView : UserControl
    {
        private string JustString = string.Empty;

        public MapView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MapViewModel viewModel)
            {
                if (viewModel.CheckFileExistence())
                {
                    await MapWebView.EnsureCoreWebView2Async();

                    MapWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                        "app.local",
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebDesign"),
                        CoreWebView2HostResourceAccessKind.Allow);

                    MapWebView.CoreWebView2.PermissionRequested += (sender, e) =>
                    {
                        if (e.PermissionKind == CoreWebView2PermissionKind.Geolocation)
                            e.State = CoreWebView2PermissionState.Allow;
                    };

                    MapWebView.WebMessageReceived += (sender, e) =>
                    {
                        var message = e.TryGetWebMessageAsString();

                        if(message == null)
                        {
                            MessageBox.Show("Не удалось разобрать сообщение!");
                            return;
                        }

                        viewModel.SetUserMarker(message);
                    };

                    MapWebView.Source = viewModel.WebViewUri;
                    viewModel.MapService = new MapService(MapWebView);
                }
            }
        }
    }
}
