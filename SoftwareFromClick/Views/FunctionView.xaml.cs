using SoftwareFromClick.Models;
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
                // TODO

                PromptPreviewPanel.Visibility = Visibility.Visible;
                _isPromptVisible = true;
            }
        }

        private async void GenerateCodeButton_Click(object sender, RoutedEventArgs e)
        {
            // ZBIERANIE DANYCH Z FORMULARZA
            string funcName = FunctionNameTextBox.Text.Trim();
            string funcDesc = FunctionalitiesTextBox.Text.Trim();
            string inputParams = InputParametersTextBox.Text.Trim();
            string returnType = ReturnTypeTextBox.Text.Trim();

            // Pobranie wartości z ComboBoxa
            var selectedTypeItem = FunctionTypeComboBox.SelectedItem as ComboBoxItem;
            string funcType = selectedTypeItem?.Content.ToString() ?? "public";

            // POBRANIE KONTEKSTU Z GŁÓWNEGO OKNA
            // Używamy referencji do MainWindow, aby dostać się do wybranych opcji
            var selectedModel = _mainWindow.ModelComboBox.SelectedItem as AiModel;
            var selectedLanguage = _mainWindow.LanguageComboBox.SelectedItem as Language;

            // WALIDACJA
            if (string.IsNullOrWhiteSpace(funcName) || string.IsNullOrWhiteSpace(funcDesc))
            {
                MessageBox.Show("Function Name and Functionalities are required!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedModel == null || selectedLanguage == null)
            {
                MessageBox.Show("Model or Language is not selected in the main screen.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // PAKOWANIE DANYCH (PRZYGOTOWANIE DO WYSŁANIA)
            var requestData = new FunctionRequestDto
            {
                FunctionName = funcName,
                FunctionType = funcType,
                Functionalities = funcDesc,
                InputParameters = inputParams,
                ReturnType = returnType,
                SelectedModel = selectedModel,
                SelectedLanguage = selectedLanguage
            };

            // WYSŁANIE DO SERWISU
            Loading.Visibility = Visibility.Visible;

            try
            {
                // NA RAZIE TYLKO TESTOWO
                OpenAiService service = new OpenAiService();

                // Tymczasowo stare wywołanie (nie używa jeszcze wszystkich pól):
                string generatedCode = await service.GetCodeFromAiAsync(
                    requestData.FunctionName,
                    requestData.ReturnType,
                    requestData.InputParameters,
                    requestData.Functionalities
                );

                ResultsView resultsView = new ResultsView(_mainWindow, generatedCode);
                _mainWindow.MainContentControl.Content = resultsView;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                Loading.Visibility = Visibility.Collapsed;
            }
        }

    }
}
