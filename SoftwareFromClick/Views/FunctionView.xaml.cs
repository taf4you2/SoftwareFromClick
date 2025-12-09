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
            // 1. ZBIERANIE DANYCH Z FORMULARZA (To pozostaje bez zmian)
            string funcName = FunctionNameTextBox.Text.Trim();
            string funcDesc = FunctionalitiesTextBox.Text.Trim();
            string inputParams = InputParametersTextBox.Text.Trim();
            string returnType = ReturnTypeTextBox.Text.Trim();

            // Pobranie wartości z ComboBoxa
            var selectedTypeItem = FunctionTypeComboBox.SelectedItem as ComboBoxItem;
            string funcType = selectedTypeItem?.Content.ToString() ?? "public";

            // POBRANIE KONTEKSTU Z GŁÓWNEGO OKNA
            var selectedModel = _mainWindow.ModelComboBox.SelectedItem as AiModel;
            var selectedLanguage = _mainWindow.LanguageComboBox.SelectedItem as Language;

            // 2. WALIDACJA (Bez zmian)
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

            // 3. PAKOWANIE DANYCH (Obiekt DTO jest już gotowy w Twoim projekcie)
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

            // 4. WYSŁANIE DO SERWISU (TUTAJ ZMIANA)
            Loading.Visibility = Visibility.Visible;

            try
            {
                OpenAiService service = new OpenAiService();

                // Używamy nowej metody ProcessFunctionRequestAsync, która:
                // - Zapisuje Question w bazie
                // - Pobiera PromptTemplate i wypełnia go
                // - Zapisuje historię do pliku JSON
                // - Wysyła request do AI
                // - Zapisuje Result w bazie
                string result = await service.ProcessFunctionRequestAsync(requestData);

                // Sprawdzenie czy serwis zwrócił błąd (Twoja metoda zwraca string z błędem lub kodem)
                if (result.StartsWith("Error") || result.StartsWith("Connection Error"))
                {
                    MessageBox.Show(result, "Generation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    // Sukces - przekazujemy wygenerowany kod do widoku wyników
                    ResultsView resultsView = new ResultsView(_mainWindow, result);
                    _mainWindow.MainContentControl.Content = resultsView;

                    // NOWE: Odśwież historię w głównym oknie!
                    _mainWindow.LoadHistory();

                    _mainWindow.MainContentControl.Content = resultsView;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Critical Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Loading.Visibility = Visibility.Collapsed;
            }
        }

    }
}
