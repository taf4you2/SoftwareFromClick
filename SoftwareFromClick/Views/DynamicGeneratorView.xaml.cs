using SoftwareFromClick.Models;
using SoftwareFromClick.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SoftwareFromClick.Views
{
    public partial class DynamicGeneratorView : UserControl
    {
        private MainWindow _mainWindow;
        private PromptTemplate _currentTemplate;

        // Mapuje identyfikatory placeholderów na utworzone kontrolki.
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

                string jsonContent = await File.ReadAllTextAsync(_currentTemplate.JsonFilePath);
                var templateDto = JsonSerializer.Deserialize<GeneratorTemplateDto>(jsonContent);

                if (templateDto == null) return;

                // Ekstrahuje pola formularza z szablonów system i user.
                var fields = _parserService.ExtractFields(templateDto.System, templateDto.User);
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
                StackPanel fieldPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };

                // Tworzy i dodaje etykietę pola.
                TextBlock label = new TextBlock
                {
                    Text = field.Label + ":",
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                fieldPanel.Children.Add(label);

                Control inputControl = null;
                Border border = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) };

                if (field.Type == InputFieldType.Text)
                {
                    // Konfiguruje pole tekstowe z obsługą wielu linii dla opisów.
                    var textBox = new TextBox
                    {
                        Height = field.Label.ToLower().Contains("description") ? 80 : 30,
                        AcceptsReturn = true,
                        TextWrapping = TextWrapping.Wrap,
                        BorderThickness = new Thickness(0),
                        Padding = new Thickness(2)
                    };
                    inputControl = textBox;
                    border.Child = inputControl;
                    fieldPanel.Children.Add(border);
                }
                else if (field.Type == InputFieldType.Choice)
                {
                    // Tworzy listę rozwijaną z opcjami z definicji pola.
                    var comboBox = new ComboBox
                    {
                        ItemsSource = field.Options,
                        Height = 30,
                        BorderThickness = new Thickness(0),
                        SelectedIndex = 0
                    };
                    inputControl = comboBox;
                    border.Child = inputControl;
                    fieldPanel.Children.Add(border);
                }
                else if (field.Type == InputFieldType.Boolean)
                {
                    // Dodaje pole wyboru (checkbox).
                    var checkBox = new CheckBox
                    {
                        Content = "Enable",
                        IsChecked = false,
                        Height = 30,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(5, 0, 0, 0)
                    };
                    inputControl = checkBox;
                    fieldPanel.Children.Add(inputControl);
                }
                else if (field.Type == InputFieldType.ParameterList)
                {
                    var collection = new ObservableCollection<ParameterItem>();

                    var dataGrid = new DataGrid
                    {
                        ItemsSource = collection,
                        AutoGenerateColumns = false,
                        CanUserAddRows = false,
                        Height = 150,
                        BorderThickness = new Thickness(1),
                        Background = Brushes.White,
                        HeadersVisibility = DataGridHeadersVisibility.Column,
                        GridLinesVisibility = DataGridGridLinesVisibility.All,
                        VerticalGridLinesBrush = Brushes.Black,
                        HorizontalGridLinesBrush = Brushes.Black
                    };

                    var typeColumn = new DataGridTemplateColumn
                    {
                        Header = "Type",
                        Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                    };

                    var typeTemplate = new DataTemplate();
                    var comboBox = new FrameworkElementFactory(typeof(ComboBox));

                    comboBox.SetValue(ComboBox.ItemsSourceProperty, Enum.GetValues(typeof(ParameterType)));

                    comboBox.SetBinding(ComboBox.SelectedItemProperty, new Binding("Type")
                    {
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    });

                    typeTemplate.VisualTree = comboBox;
                    typeColumn.CellTemplate = typeTemplate;

                    var nameColumn = new DataGridTextColumn
                    {
                        Header = "Name",
                        Width = new DataGridLength(2, DataGridLengthUnitType.Star),
                        Binding = new Binding("Name")
                    };

                    var deleteColumn = new DataGridTemplateColumn
                    {
                        Header = "Actions",
                        Width = 70
                    };

                    var template = new DataTemplate();
                    var deleteButton = new FrameworkElementFactory(typeof(Button));
                    deleteButton.SetValue(Button.ContentProperty, "Delete");
                    deleteButton.SetValue(Button.WidthProperty, 70.0);


                    deleteButton.AddHandler(Button.ClickEvent, new RoutedEventHandler((s, e) =>
                    {
                        if (s is Button btn && btn.DataContext is ParameterItem itemToRemove)
                        {
                            collection.Remove(itemToRemove);
                            e.Handled = true;
                        }
                    }));

                    template.VisualTree = deleteButton;
                    deleteColumn.CellTemplate = template;

                    dataGrid.Columns.Add(typeColumn);
                    dataGrid.Columns.Add(nameColumn);
                    dataGrid.Columns.Add(deleteColumn);


                    var addButton = new Button
                    {
                        Content = "+ Add Parameter",
                        Height = 22,
                        Padding = new Thickness(10, 0, 10, 0),
                        Background = new SolidColorBrush(Color.FromRgb(240, 240, 240))
                    };

                    addButton.Click += (s, e) =>
                    {
                        collection.Add(new ParameterItem());
                    };

                    border.Child = dataGrid;
                    var container = new StackPanel();
                    container.Children.Add(border);
                    container.Children.Add(addButton);

                    fieldPanel.Children.Add(container);


                    inputControl = dataGrid;
                }

                if (!DynamicFormPanel.Children.Contains(fieldPanel))
                {
                    DynamicFormPanel.Children.Add(fieldPanel);
                }

                if (inputControl != null && !_inputControls.ContainsKey(field.Placeholder))
                {
                    _inputControls.Add(field.Placeholder, inputControl);
                }
            }
        }

        private async void GenerateCodeButton_Click(object sender, RoutedEventArgs e)
        {
            var placeholders = new Dictionary<string, string>();

            foreach (var entry in _inputControls)
            {
                string value = string.Empty;

                if (entry.Value is TextBox tb)
                {
                    value = tb.Text.Trim();
                    // Przerywa działanie, jeśli wymagane pole tekstowe jest puste
                    if (string.IsNullOrEmpty(value))
                    {
                        MessageBox.Show($"Field {entry.Key} is empty.");
                        return;
                    }
                }
                else if (entry.Value is ComboBox cb && !(entry.Value is DataGrid))
                {
                    value = cb.SelectedValue?.ToString() ?? string.Empty;
                }
                else if (entry.Value is CheckBox chk)
                {
                    value = chk.IsChecked == true ? "true" : "false";
                }
                else if (entry.Value is DataGrid dg)
                {
                    // Przetwarza dane z tabeli parametrów na format tekstowy
                    if (dg.ItemsSource is ObservableCollection<ParameterItem> items)
                    {
                        var paramsList = items
                            .Where(i => !string.IsNullOrWhiteSpace(i.Name))
                            .Select(i => $"{i.Type.ToString().ToLower()} {i.Name}");

                        value = string.Join(", ", paramsList);
                    }
                }

                placeholders.Add(entry.Key, value);
            }

            var selectedModel = _mainWindow.ModelComboBox.SelectedItem as AiModel;
            var selectedLanguage = _mainWindow.LanguageComboBox.SelectedItem as Language;

            if (selectedModel == null || selectedLanguage == null) return;

            // Ustawia tytuł historii na podstawie nazwy wpisu lub szablonu
            string titleToSave = _currentTemplate.Name;
            var nameEntry = placeholders.FirstOrDefault(p => p.Key.Contains("Name", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(nameEntry.Key)) titleToSave = nameEntry.Value;

            Loading.Visibility = Visibility.Visible;
            try
            {
                // Wysyła żądanie generowania kodu do serwisu AI
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
                    // Przekazuje wynik do widoku rezultatów i odświeża historię
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