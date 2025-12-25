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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SoftwareFromClick.Views
{
    /// <summary>
    /// Interaction logic for TemplateManagerView.xaml
    /// </summary>
    public partial class TemplateManagerView : UserControl
    {
        private MainWindow _mainWindow;

        public TemplateManagerView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowWelcomeScreen();
        }

        private void SaveTemplateButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteTemplateButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
