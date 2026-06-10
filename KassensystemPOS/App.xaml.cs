using System.Windows;
using System.Windows.Threading;

namespace KassensystemPOS
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Sicherheitsnetz: faengt ALLE unbehandelten Fehler ab (z.B. wenn die
            // Cloud-Datenbank kurz nicht erreichbar ist) und zeigt eine Meldung,
            // statt die ganze Anwendung abstuerzen zu lassen.
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                "Es ist ein Fehler aufgetreten:\n\n" + e.Exception.Message,
                "Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true; // Fehler als behandelt markieren -> App laeuft weiter
        }
    }
}
