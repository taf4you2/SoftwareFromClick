using System.Text;
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
using SoftwareFromClick.Services;

namespace SoftwareFromClick.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private UIElement _welcomeScreen; // 'snap' ekranu poczatkowego
        private readonly MainService _mainService; // Referencja do serwisu


        public MainWindow()
        {
            InitializeComponent();
            _welcomeScreen = MainContentControl.Content as UIElement; // zapis ekranu powitalnego (zeby sie dalo wrocic)

            // Inicjalizacja serwisu
            _mainService = new MainService();

            // Ładowanie danych do list rozwijanych
            LoadComboBoxData();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Show();

            //settingsWindow.ShowDialog(); // okno dialogowe trzeba zamknąć żeby zrobić cokolwiek w głównym oknie
            // żeby okno dialogowe działało w przyciskach trzeba zrobić zwort true albo false albo jaki kolwiek (chyba)
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem selectedTypeItem = TypeComboBox.SelectedItem as ComboBoxItem;
            string selectedType = selectedTypeItem?.Content.ToString();

            var selectedModel = ModelComboBox.SelectedItem as AiModel;
            var selectedLanguage = LanguageComboBox.SelectedItem as Language;

            if (selectedType != null && selectedModel != null && selectedLanguage != null)
            {
                if (selectedType == "Function")
                {
                    // tu przerzucamy ustawienia poczatkowe do functionview
                    FunctionView functionView = new FunctionView(this);
                    MainContentControl.Content = functionView;
                }
                else if (selectedType == "Class")
                {
                    MessageBox.Show("ClassView not implemented yet");
                }
            }
            else
            {
                MessageBox.Show("Please select all options (Model, Language, Type)");
            }
        }

        private void LoadComboBoxData()
        {
            try
            {
                // 1. Pobierz dane z serwisu
                var languages = _mainService.GetLanguages();
                var models = _mainService.GetAiModels();

                // 2. Przypisz do ComboBoxów
                LanguageComboBox.ItemsSource = languages;
                ModelComboBox.ItemsSource = models;

                // 3. Opcjonalnie: Zaznacz pierwsze elementy domyślnie
                if (LanguageComboBox.Items.Count > 0) LanguageComboBox.SelectedIndex = 0;
                if (ModelComboBox.Items.Count > 0) ModelComboBox.SelectedIndex = 0;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Błąd ładowania danych z bazy: " + ex.Message);
            }
        }

        public void ShowWelcomeScreen()
        {
            MainContentControl.Content = _welcomeScreen; // po nacisnieciu back przywrocenie stanu poczatkowego
        }

        public void ShowResultView()
        {
            ResultsView resultView = new ResultsView(this, string.Empty); // trzeba tak zrobić żeby ekrany między sobą przekazywały tego prompta
            MainContentControl.Content = resultView;
        }

    }
}