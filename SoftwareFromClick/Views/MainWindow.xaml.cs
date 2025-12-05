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

namespace SoftwareFromClick.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private UIElement _welcomeScreen; // 'snap' ekranu poczatkowego

        public MainWindow()
        {
            InitializeComponent();
            _welcomeScreen = MainContentControl.Content as UIElement; // zapis ekranu powitalnego (zeby sie dalo wrocic)
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem selectedType = TypeComboBox.SelectedItem as ComboBoxItem;

            if (selectedType != null)
            {
                if (selectedType.Content.ToString() == "Function")
                {
                    FunctionView functionView = new FunctionView(this);
                    MainContentControl.Content = functionView;
                }
                else if (selectedType.Content.ToString() == "Class")
                {
                    MessageBox.Show("ClassView not implemented yet");
                }
            }
            else
            {
                MessageBox.Show("Please select all options");
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