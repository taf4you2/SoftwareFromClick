using SoftwareFromClick.Data;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;


namespace SoftwareFromClick
{
        /// <summary>
        /// Interaction logic for App.xaml e
        /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                using (var context = new AppDbContext())
                {
                    // Tworzy bazę jeśli jej nie ma.
                    context.Database.Migrate();
                }
            }
            catch (System.Exception ex)
            {

                MessageBox.Show($"Błąd inicjalizacji: {ex.Message}", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(); // Zamknij aplikację, bo bez bazy nie zadziała
            }
        }
    }
}