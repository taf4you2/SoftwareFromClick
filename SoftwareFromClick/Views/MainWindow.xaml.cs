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
using System.Windows;

namespace SoftwareFromClick.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
            //settingsWindow.Show(); 
            /*
                roznica jest kosmetyczna a jednak fundamentalna
                bo w tym pierwszym nie mozna kilikac w glownym oknie w nic a w drugim mozna
                i jest to ciekawe
             */

        }
    }
}