using AssemblyUpdater.Models;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AssemblyUpdater
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            DataContext = new MainWindowPageViewModel();

            InitializeComponent();
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private void VersionTextChanged(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Update_Single_Assembly(object sender, RoutedEventArgs e)
        {
            AssemblyFileItem item = (sender as Button).DataContext as AssemblyFileItem;
            MainWindowPageViewModel model = new MainWindowPageViewModel();
            string newVersion = model.GetReadableVersion(model.ToWriteVersion);
            model.UpdateSingleFileWithNotification(item, newVersion);
        }
    }
}
