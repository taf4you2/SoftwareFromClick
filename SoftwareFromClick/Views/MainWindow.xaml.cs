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

        private readonly TemplateService _templateService;


        public MainWindow()
        {
            InitializeComponent();
            _welcomeScreen = MainContentControl.Content as UIElement;

            // INICJALIZACJA SERWISÓW
            _mainService = new MainService();
            _templateService = new TemplateService();

            // PODPINANIE ZDARZEŃ
            LanguageComboBox.SelectionChanged += LanguageComboBox_SelectionChanged;

            // ŁADOWANIE DANYCH
            LoadComboBoxData();
            LoadHistory();
        }

        private void TemplatesButton_Click(object sender, RoutedEventArgs e)
        {
            TemplateManagerView templateView = new TemplateManagerView(this);
            MainContentControl.Content = templateView;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsView settingsView = new SettingsView(this);
            MainContentControl.Content = settingsView;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Walidacja wyboru
            var selectedModel = ModelComboBox.SelectedItem as AiModel;
            var selectedLanguage = LanguageComboBox.SelectedItem as Language;
            var selectedTemplate = TemplateComboBox.SelectedItem as PromptTemplate;

            if (selectedModel == null || selectedLanguage == null || selectedTemplate == null)
            {
                MessageBox.Show("Please select Model, Language and Template.");
                return;
            }

            // tworzenie nowego widoku przez przekazanie
            DynamicGeneratorView dynamicView = new DynamicGeneratorView(this, selectedTemplate);
            MainContentControl.Content = dynamicView;
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

        public void LoadHistory()
        {
            try
            {
                var history = _mainService.GetHistory();
                HistoryListView.ItemsSource = history;
            }
            catch (System.Exception ex)
            {
                // Ciche łapanie błędu przy starcie, żeby nie straszyć usera pustą bazą
                Console.WriteLine(ex.Message);
            }
        }

        private void HistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryListView.SelectedItem is Question selectedQuestion)
            {
                // Sprawdzamy, czy to zapytanie ma jakieś wyniki
                var lastResult = selectedQuestion.Results
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefault();

                if (lastResult != null && !string.IsNullOrEmpty(lastResult.JsonFilePath))
                {
                    // Pobieramy treść kodu z pliku JSON
                    string code = _mainService.GetCodeFromResult(lastResult.JsonFilePath);

                    // Wyświetlamy ResultsView z historycznym kodem
                    ResultsView resultsView = new ResultsView(this, code);
                    MainContentControl.Content = resultsView;
                }
                else
                {
                    MessageBox.Show("No results found for this query.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Opcjonalnie: Resetujemy zaznaczenie, żeby można było kliknąć to samo jeszcze raz
                HistoryListView.SelectedItem = null;
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is Language selectedLanguage)
            {
                // Pobierz wszystkie szablony i przefiltruj po wybranym języku
                var allTemplates = _templateService.GetAllTemplates();
                var filteredTemplates = allTemplates
                                        .Where(t => t.LanguageId == selectedLanguage.Id)
                                        .ToList();

                TemplateComboBox.ItemsSource = filteredTemplates;

                // Ustaw pierwszy domyślnie, jeśli istnieje
                if (filteredTemplates.Count > 0)
                {
                    TemplateComboBox.SelectedIndex = 0;
                }
                else
                {
                    TemplateComboBox.ItemsSource = null;
                }
            }
        }

    }
}