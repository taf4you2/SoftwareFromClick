using SoftwareFromClick.Models;
using SoftwareFromClick.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using System.IO;

namespace SoftwareFromClick.Views
{
    /// <summary>
    /// Interaction logic for DynamicGeneratorView.xaml
    /// </summary>
    public partial class DynamicGeneratorView : UserControl
    {
        private MainWindow _mainWindow;
        private PromptTemplate _currentTemplate;

        // Słownik mapujący placeholder na kontrolkę, która trzyma wartość
        private Dictionary<string, Control> _inputControls = new Dictionary<string, Control>();

        private readonly TemplateParserService _parserService;
        private readonly OpenAiService _openAiService;

        public DynamicGeneratorView(MainWindow mainWindow, PromptTemplate template)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _currentTemplate = template;

            _parserService = new TemplateParserService();
            _openAiService = new OpenAiService();

            // Ustawienie nazwy funkcji jako nazwę szablonu
            HeaderTextBlock.Text = $"Generate: {_currentTemplate.Name}";

            LoadDynamicInterface();
        }

        private async void LoadDynamicInterface()
        {
            try
            {
                if (!File.Exists(_currentTemplate.JsonFilePath))
                {
                    MessageBox.Show("Template file not found!");
                    return;
                }

                // Wczytanie treści JSON szablonu
                string jsonContent = await File.ReadAllTextAsync(_currentTemplate.JsonFilePath);
                var templateDto = JsonSerializer.Deserialize<GeneratorTemplateDto>(jsonContent);

                if (templateDto == null) return;

                // Wykrycie pól
                var fields = _parserService.ExtractFields(templateDto.System, templateDto.User);

                // Generowanie pól
                GenerateControls(fields);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading template: {ex.Message}");
            }
        }


        private void GenerateControls(List<TemplateField> fields)
        {
            DynamicFormPanel.Children.Clear();
            _inputControls.Clear();

            foreach (var field in fields)
            {
                // Kontener dla pojedynczego pola
                StackPanel fieldPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };

                // Kontrolka wejściowa 
                Control inputControl = null;
                Border border = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) };

                if (field.Type == InputFieldType.Choice)
                {
                    // Etykieta (Label)
                    TextBlock label = new TextBlock
                    {
                        Text = field.Label + ":",
                        FontWeight = FontWeights.SemiBold,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    fieldPanel.Children.Add(label);

                    // Tworzenie ComboBox
                    var comboBox = new ComboBox
                    {
                        ItemsSource = field.Options,
                        Height = 25,
                        BorderThickness = new Thickness(0),
                        SelectedIndex = 0 // Domyślnie pierwsza opcja
                    };
                    inputControl = comboBox;
                }
                else if (field.Type == InputFieldType.Boolean)
                {
                    // Tworzenie Checkbox
                    var checkBox = new CheckBox
                    {
                        Content = field.Label,
                        IsChecked = false,
                        Height = 25,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(5, 0, 0, 0)
                    };
                    inputControl = checkBox;
                }
                else
                {
                    // Etykieta (Label)
                    TextBlock label = new TextBlock
                    {
                        Text = field.Label + ":",
                        FontWeight = FontWeights.SemiBold,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    fieldPanel.Children.Add(label);

                    // Tworzenie TextBox
                    var textBox = new TextBox
                    {
                        Height = field.Label.ToLower().Contains("description") || field.Label.ToLower().Contains("content") ? 80 : 30, // Większe pole dla opisów
                        AcceptsReturn = true,
                        TextWrapping = TextWrapping.Wrap,
                        BorderThickness = new Thickness(0),
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Padding = new Thickness(2)
                    };
                    inputControl = textBox;
                }

                // Dodanie kontrolki do obramowania i panelu
                border.Child = inputControl;
                fieldPanel.Children.Add(border);

                // Dodanie panelu pola do głównego widoku
                DynamicFormPanel.Children.Add(fieldPanel);

                // Zapisanie referencji w słowniku
                // Kluczem jest Placeholder (np. "{{Author}}"), wartością TextBox/ComboBox
                _inputControls.Add(field.Placeholder, inputControl);
            }
        }

        private async void GenerateCodeButton_Click(object sender, RoutedEventArgs e)
        {
            // Zbieranie danych z formularza
            var placeholders = new Dictionary<string, string>();

            foreach (var entry in _inputControls)
            {
                string value = string.Empty;

                if (entry.Value is TextBox tb)
                {
                    value = tb.Text.Trim();
                    if (string.IsNullOrEmpty(value))
                    {
                        MessageBox.Show("Please fill in all fields.");
                        return;
                    }
                }
                else if (entry.Value is ComboBox cb)
                {
                    value = cb.SelectedValue?.ToString() ?? string.Empty;
                }
                else if (entry.Value is CheckBox chk)
                {
                    // Zamienia zaznaczenie na tekst "true" lub "false"
                    value = chk.IsChecked == true ? "true" : "false";
                }

                placeholders.Add(entry.Key, value);
            }

            // Walidacja
            var selectedModel = _mainWindow.ModelComboBox.SelectedItem as AiModel;
            var selectedLanguage = _mainWindow.LanguageComboBox.SelectedItem as Language;

            if (selectedModel == null || selectedLanguage == null)
            {
                MessageBox.Show("Please select Model and Language in the main window sidebar.");
                return;
            }

            // Domyślnie używa nazwy szablonu
            string titleToSave = _currentTemplate.Name;

            // Szuka klucza zawierającego frazę "Name"
            var nameEntry = placeholders.FirstOrDefault(p => p.Key.Contains("Name", StringComparison.OrdinalIgnoreCase));

            // Sprawdza czy znaleziono pasujący klucz i czy ma wartość
            if (!string.IsNullOrEmpty(nameEntry.Key))
            {
                titleToSave = nameEntry.Value;
            }
            else
            {
                // Alternatywnie: pobiera wartość z pierwszego pola tekstowego jeśli nie znaleziono "Name"
                var firstValue = placeholders.Values.FirstOrDefault();
                if (!string.IsNullOrEmpty(firstValue))
                {
                    titleToSave = firstValue.Length > 20 ? firstValue.Substring(0, 20) + "..." : firstValue;
                }
            }

            // Wysłanie żądania
            Loading.Visibility = Visibility.Visible;
            try
            {
                string result = await _openAiService.ProcessGenerationRequestAsync(
                    titleToSave,
                    _currentTemplate.TemplateType,
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
                MessageBox.Show($"Critical Error: {ex.Message}");
            }
            finally
            {
                Loading.Visibility = Visibility.Collapsed;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowWelcomeScreen();
        }
    }
}
