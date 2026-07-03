using GearFix.ViewModels;
using System.Windows.Controls;

namespace GearFix.Views
{
    /// <summary>
    /// Логика взаимодействия для MenuView.xaml
    /// </summary>
    public partial class MenuView : UserControl
    {
        public MenuView()
        {
            InitializeComponent();

            if (DataContext is MenuViewModel viewModel)
            {
                viewModel.CheckApiKey();
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
