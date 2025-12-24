using SoftwareFromClick.Models;
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
    /// Interaction logic for ClassView.xaml
    /// </summary>
    public partial class ClassView : UserControl
    {
        private MainWindow _mainWindow;
        private bool _isPromptVisible = false;

        public ClassView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowWelcomeScreen();
        }

        private void PreviewPromptButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isPromptVisible)
            {

                PromptPreviewPanel.Visibility = Visibility.Collapsed;
                _isPromptVisible = false;
            }
            else
            {
                // TODO

                PromptPreviewPanel.Visibility = Visibility.Visible;
                _isPromptVisible = true;
            }
        }

        private async void GenerateCodeButton_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
