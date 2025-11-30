using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SoftwareFromClick.Views
{
    /// <summary>
    /// Interaction logic for ResultsView.xaml
    /// </summary>
    public partial class ResultsView : UserControl
    {
        private MainWindow _mainWindow;

        public ResultsView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }
        private void NewQueryButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowWelcomeScreen();
        }

        private void EditQueryButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("not implemented");
        }
    }
}
