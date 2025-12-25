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
using System.Windows.Navigation;
using System.Windows.Shapes;
using SoftwareFromClick.Models;


namespace SoftwareFromClick.Views
{
    /// <summary>
    /// Interaction logic for TemplateManagerView.xaml
    /// </summary>
    public partial class TemplateManagerView : UserControl
    {
        private MainWindow _mainWindow;

        private readonly MainService _mainService;
        private readonly TemplateService _templateService;

        public TemplateManagerView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            _mainService = new MainService();
            _templateService = new TemplateService();

            LoadInitialData();
        }

        // Metoda ładująca wszystkie potrzebne dane
        private void LoadInitialData()
        {
            try
            {
                // ComboBoxa językami
                var languages = _mainService.GetLanguages();
                LanguageComboBox.ItemsSource = languages;

                // Ustawienie domyślnego wyboru na pierwszy język
                if (languages.Count > 0)
                {
                    LanguageComboBox.SelectedIndex = 0;
                }

                // Wypełnienie tabeli DataGrid
                LoadTemplatesList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading initial data: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Metoda odświeżająca listę szablonów w tabeli po lewej stronie
        private void LoadTemplatesList()
        {
            try
            {
                // Pobieramy listę z TemplateService
                TemplatesDataGrid.ItemsSource = _templateService.GetAllTemplates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading templates: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- OBSŁUGA PRZYCISKÓW ---

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowWelcomeScreen();
        }

        private async void SaveTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            // Pobranie danych z kontrolek
            string name = TemplateNameTextBox.Text.Trim();

            // Pobranie typu z ComboBoxa
            var selectedTypeItem = TemplateTypeComboBox.SelectedItem as ComboBoxItem;
            string type = selectedTypeItem?.Content.ToString() ?? "Function";

            // Pobranie języka
            var selectedLanguage = LanguageComboBox.SelectedItem as Language;

            string systemPrompt = SystemPromptTextBox.Text.Trim();
            string userPrompt = UserPromptTextBox.Text.Trim();

            // Prosta walidacja
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a Template Name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (selectedLanguage == null)
            {
                MessageBox.Show("Please select a Programming Language.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(userPrompt))
            {
                MessageBox.Show("User Prompt cannot be empty.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Próba zapisu
            try
            {
                // Blokujemy przycisk na chwilę, żeby użytkownik nie kliknął dwa razy
                (sender as Button).IsEnabled = false;

                await _templateService.AddTemplateAsync(name, type, selectedLanguage.Id, systemPrompt, userPrompt);

                MessageBox.Show("Template saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Wyczyszczenie
                TemplateNameTextBox.Text = string.Empty;
                SystemPromptTextBox.Text = string.Empty;
                UserPromptTextBox.Text = string.Empty;

                // Odświeżenie DataGrid
                LoadTemplatesList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical Error saving template: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Odblokowanie przycisku
                (sender as Button).IsEnabled = true;
            }
        }

        // Usuwanie szablonu
        private void DeleteTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int templateId)
            {
                // Potwierdzenie
                var result = MessageBox.Show("Are you sure you want to delete this template?\nThis will remove the template record from database.",
                                             "Confirm Delete",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Wywołanie usuwania
                        _templateService.DeleteTemplate(templateId);

                        // Odświeżenie DataGrid
                        LoadTemplatesList();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting template: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
