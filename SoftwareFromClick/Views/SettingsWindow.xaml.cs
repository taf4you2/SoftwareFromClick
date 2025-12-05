using SoftwareFromClick.Data;
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
using SoftwareFromClick.Models;
using SoftwareFromClick.Services;




namespace SoftwareFromClick.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        // Deklaracja serwisu
        private readonly ApiKeyService _apiKeyService;

        public SettingsWindow()
        {
            InitializeComponent();

            // Inicjalizacja serwisu
            _apiKeyService = new ApiKeyService();

            // Załadowanie danych przy starcie
            LoadApiKeys();
        }

        // Metoda pomocnicza do odświeżania tabeli
        private void LoadApiKeys()
        {
            // Pobieramy dane z serwisu i wkładamy do DataGrida
            KeysDataGrid.ItemsSource = _apiKeyService.GetAllKeys();
        }

        // PRZYCISK 1: ADD (Dodaj i Zapisz od razu)
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Pobranie danych z formularza
            string newKey = KeyTextBox.Text.Trim();
            // Bezpieczne pobranie tekstu z ComboBoxa
            string providerName = (ProviderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Unknown";

            // Walidacja - czy pole nie jest puste
            if (string.IsNullOrEmpty(newKey))
            {
                MessageBox.Show("Please enter an API Key.");
                return;
            }

            try
            {
                // Wywołanie serwisu (to on zapisuje do bazy)
                _apiKeyService.AddApiKey(newKey, providerName);

                // Wyczyszczenie pola tekstowego
                KeyTextBox.Text = string.Empty;

                // Odświeżenie listy, żebyś od razu widział dodany klucz
                LoadApiKeys();

                MessageBox.Show("Key added successfully!");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error adding key: {ex.Message}");
            }
        }

        // save jest do wycięcia tylko muszę się zastanowić jak to zrobić

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Zamyka okno bez zapisywania
        }
        // Dodana brakująca metoda do usuwania
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Pobieramy ID z właściwości Tag przycisku (ustawione w XAML przez Binding)
            if (sender is Button btn && btn.Tag is int keyId)
            {
                if (MessageBox.Show("Are you sure you want to delete this key?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _apiKeyService.DeleteApiKey(keyId);
                        LoadApiKeys();
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Error deleting key: {ex.Message}");
                    }
                }
            }
        }
    }
}
