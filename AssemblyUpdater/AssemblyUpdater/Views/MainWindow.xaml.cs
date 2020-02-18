using System.Text.RegularExpressions;
using System.Windows;
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




    }
}
