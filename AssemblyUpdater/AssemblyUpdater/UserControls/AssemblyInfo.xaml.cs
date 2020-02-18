using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AssemblyUpdater.UserControls
{
    /// <summary>
    /// Interaction logic for AssemblyInfo.xaml
    /// </summary>
    public partial class AssemblyInfo : UserControl
    {
        public AssemblyInfo()
        {
            InitializeComponent();
        }

        private void VersionTextChanged(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

    }
}
