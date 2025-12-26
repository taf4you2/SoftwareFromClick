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
            // Pobranie kontekstu
            var selectedModel = _mainWindow.ModelComboBox.SelectedItem as AiModel;
            var selectedLanguage = _mainWindow.LanguageComboBox.SelectedItem as Language;

            // Pobranie danych z formularza
            string funcName = FunctionNameTextBox.Text.Trim();
            string funcDesc = FunctionalitiesTextBox.Text.Trim();
            string inputParams = InputParametersTextBox.Text.Trim();
            string returnType = ReturnTypeTextBox.Text.Trim();

            var selectedTypeItem = FunctionTypeComboBox.SelectedItem as ComboBoxItem;
            string funcType = selectedTypeItem?.Content.ToString() ?? "public";

            // Walidacja
            if (selectedModel == null || selectedLanguage == null)
            {
                MessageBox.Show("Select Model and Language first.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(funcName) || string.IsNullOrWhiteSpace(funcDesc))
            {
                MessageBox.Show("Function Name and Description are required!", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Budowanie słownika placeholderów
            // Klucze muszą odpowiadać tym w szablonie JSON w bazie danych
            var placeholders = new Dictionary<string, string>
            {
                { "{{FunctionName}}", funcName },
                { "{{FunctionType}}", funcType },
                { "{{ReturnType}}", returnType },
                { "{{InputParameters}}", inputParams },
                { "{{FunctionalityDescription}}", funcDesc }
            };

            // Wywołanie API Ai
            Loading.Visibility = Visibility.Visible;
            try
            {
                OpenAiService service = new OpenAiService();

                string result = await service.ProcessGenerationRequestAsync(
                    funcName,
                    "Function",
                    selectedLanguage,
                    selectedModel,
                    placeholders
                );

                if (result.StartsWith("Error"))
                {
                    MessageBox.Show(result, "Generation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    ResultsView resultsView = new ResultsView(_mainWindow, result);
                    _mainWindow.MainContentControl.Content = resultsView;
                    _mainWindow.LoadHistory();
                }
            }
            catch (Exception ex)
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
