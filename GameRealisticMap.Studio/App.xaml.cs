using System;
using System.IO;
using System.Reflection;
using System.Windows;
using NLog;

namespace GameRealisticMap.Studio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Logger logger = LogManager.GetLogger("Application");

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += Dispatcher_UnhandledException;
            try
            {
                InitLogging();
            }
            catch
            {
                // Nlog swear to never throw exceptions by default,
                // but in doubt, ignore logging initialization errors
            }
            base.OnStartup(e);
        }

        private static void InitLogging()
        {
            var logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GameRealisticMap", "logs");

            Directory.CreateDirectory(logDirectory);

            LogManager.Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteToFile(
                    fileName: Path.Combine(logDirectory, "studio_${shortdate}.txt"),
                    layout: "${longdate}|${message:withexception=true}",
                    maxArchiveDays: 7,
                    maxArchiveFiles: 10,
                    archiveAboveSize: 1_000_000_000 // 1 MB
                    );
            });

            logger.Info("Started Version {0}", GetAppVersion());
        }

        public static string? GetAppVersion()
        {
            return typeof(App).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            logger.Error(e.Exception, "Dispatcher UnhandledException");
            ShowException(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Error(e.ExceptionObject as Exception, "CurrentDomain UnhandledException");
            ShowException((e.ExceptionObject as Exception));
        }

        private static void ShowException(Exception? e)
        {
            MessageBox.Show(GetExceptionMessage(e), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                LogManager.Shutdown();
            }
            catch
            {
                // Ignore logging errors
            }
            base.OnExit(e);
        }

        public static string GetExceptionMessage(Exception? exception)
        {
            if (exception is TargetInvocationException invocationException && invocationException.InnerException != null)
            {
                return GetExceptionMessage(invocationException.InnerException);
            }
            return exception?.Message ?? "Unknown error";
        }
    }
}
