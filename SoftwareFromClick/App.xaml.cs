using SoftwareFromClick.Data;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;



namespace SoftwareFromClick
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        // czuje że w przyszłości to co się tu dzieje wywoła bardzo dużo chaosu ale i tak to zrobię
        // funkcja do inicjalizacji bazy
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                using (var context = new AppDbContext())
                {
                    context.Database.Migrate(); // według zdobytej wiedzy jest to najlepszy sposób na inicjowanie bazy bo
                                                // gdy zdaży się sytuacja że encje będą zaktualizowane w jakiś sposób to
                                                // program dalej będzie mógł działać poprawnie czy coś
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Błąd inicjalizacji: {ex.Message}", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
