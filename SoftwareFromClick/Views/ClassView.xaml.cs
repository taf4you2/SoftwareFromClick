using SoftwareFromClick.Models;
using SoftwareFromClick.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SoftwareFromClick.Views
{
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

        // Logika pokazywania/ukrywania panelu podglądu
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
            // Pobranie kontekstu
            var selectedModel = _mainWindow.ModelComboBox.SelectedItem as AiModel;
            var selectedLanguage = _mainWindow.LanguageComboBox.SelectedItem as Language;

            if (selectedModel == null || selectedLanguage == null)
            {
                MessageBox.Show("Please select Model and Language in the main window.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Pobranie danych z formularza
            string className = ClassNameTextBox.Text.Trim();

            var modifierItem = AccessModifierComboBox.SelectedItem as ComboBoxItem;
            string modifier = modifierItem?.Content.ToString() ?? "public";

            string properties = PropertiesTextBox.Text.Trim();
            string methods = MethodsTextBox.Text.Trim();

            // Walidacja
            if (string.IsNullOrEmpty(className))
            {
                MessageBox.Show("Class Name is required!", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Budowanie słownika placeholderów
            // Klucze muszą odpowiadać tym w szablonie JSON w bazie danych
            var placeholders = new Dictionary<string, string>
            {
                { "{{ClassName}}", className },
                { "{{AccessModifier}}", modifier },
                { "{{Properties}}", properties },
                { "{{Methods}}", methods }
            };

            // Wywołanie API Ai
            Loading.Visibility = Visibility.Visible;
            try
            {
                OpenAiService service = new OpenAiService();

                string result = await service.ProcessGenerationRequestAsync(
                    className,
                    "Class",
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