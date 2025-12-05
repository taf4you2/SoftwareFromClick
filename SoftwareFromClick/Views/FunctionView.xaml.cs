using SoftwareFromClick.Services;
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
    /// Interaction logic for FunctionView.xaml
    /// </summary>
    public partial class FunctionView : UserControl
    {
        private MainWindow _mainWindow;
        private bool _isPromptVisible = false;

        public FunctionView(MainWindow mainWindow)
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
                PromptPreviewPanel.Visibility = Visibility.Visible;
                _isPromptVisible = true;
            }
        }

        private async void GenerateCodeButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Pobieramy tekst z pola, które nazwaliśmy
            string functionalityDescription = FunctionalitiesTextBox.Text;

            if (string.IsNullOrWhiteSpace(functionalityDescription))
            {
                MessageBox.Show("Please describe functionalities first.");
                return;
            }

            // 2. Pokazujemy Loading
            Loading.Visibility = Visibility.Visible;

            // 3. Wywołujemy serwis
            OpenAiService service = new OpenAiService();

            // Tutaj aplikacja poczeka na odpowiedź z sieci, ale nie zawiesi interfejsu
            string generatedCode = await service.GetCodeFromAiAsync(functionalityDescription);

            // 4. Ukrywamy Loading
            Loading.Visibility = Visibility.Collapsed;

            // 5. Przechodzimy do widoku wyników, przekazując kod
            ResultsView resultsView = new ResultsView(_mainWindow, generatedCode);
            _mainWindow.MainContentControl.Content = resultsView;
        }

    }
}
