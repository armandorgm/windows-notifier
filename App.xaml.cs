using System;
using System.IO;
using System.Windows;

namespace WindowsNotifier
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Manejo de excepciones globales
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                LogException(args.ExceptionObject as Exception);
            };

            DispatcherUnhandledException += (s, args) =>
            {
                LogException(args.Exception);
                args.Handled = true; // Prevenir el cierre abrupto
            };
        }

        private static void LogException(Exception? ex)
        {
            if (ex == null) return;
            try
            {
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_log.txt");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Exception: {ex.Message}\nStackTrace:\n{ex.StackTrace}\n\n");
                MessageBox.Show($"Ocurrió un error inesperado:\n{ex.Message}\n\nDetalles guardados en error_log.txt", "Error Crítico Notificador", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch { }
        }
    }
}


