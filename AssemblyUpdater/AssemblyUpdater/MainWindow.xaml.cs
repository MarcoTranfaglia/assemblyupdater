using System.ComponentModel;
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
            if (Settings.Default.LastWndWidth > 0)
            {
                Top = Settings.Default.LastWndTop;
                Left = Settings.Default.LastWndLeft;
                Width = Settings.Default.LastWndWidth;
                Height = Settings.Default.LastWndHeight;
            }
            else
            {
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.LastWndTop = Top;
            Settings.Default.LastWndLeft = Left;
            Settings.Default.LastWndWidth = Width;
            Settings.Default.LastWndHeight = Height;
            Settings.Default.Save();

            base.OnClosing(e);
        }

        private void VersionTextChanged(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
