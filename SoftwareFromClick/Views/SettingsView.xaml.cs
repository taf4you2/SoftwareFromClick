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
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private readonly ApiKeyService _apiKeyService;

        private MainWindow _mainWindow;

        public SettingsView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            _apiKeyService = new ApiKeyService();

            LoadApiKeys();
        }

        private void LoadApiKeys()
        {
            KeysDataGrid.ItemsSource = _apiKeyService.GetAllKeys();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string newKey = KeyTextBox.Text.Trim();
            string providerName = (ProviderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Unknown";

            if (providerName.Contains("OpenAI")) providerName = "OpenAI";

            if (string.IsNullOrEmpty(newKey))
            {
                MessageBox.Show("Please enter an API Key.");
                return;
            }

            try
            {
                _apiKeyService.AddApiKey(newKey, providerName);
                KeyTextBox.Text = string.Empty;
                LoadApiKeys();
                MessageBox.Show("Key added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding key: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowWelcomeScreen();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int keyId)
            {
                if (MessageBox.Show("Are you sure you want to delete this key?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _apiKeyService.DeleteApiKey(keyId);
                        LoadApiKeys();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting key: {ex.Message}");
                    }
                }
            }
        }
    }
}
