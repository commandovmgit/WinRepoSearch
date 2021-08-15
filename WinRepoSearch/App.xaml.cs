using System.IO;

using CommunityToolkit.Mvvm.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

using WinRepoSearch.Contracts.Services;
using WinRepoSearch.Helpers;

// To learn more about WinUI3, see: https://docs.microsoft.com/windows/apps/winui/winui3/.
namespace WinRepoSearch
{
    public partial class App : Application
    {
        private static Window mainWindow = new() { Title = "AppDisplayName".GetLocalized() };
        private static ILogger<App> _logger;

        public static IHost? ServiceHost { get; private set; }
        public static Window MainWindow
        {
            get => mainWindow;
            set => mainWindow = value;
        }

        public static LaunchActivatedEventArgs? Args { get; private set; }
        public static IActivationService ActService
            => Ioc.Default.GetService<IActivationService>()!;
        public static ILogger Logger
            => _logger ??= Ioc.Default.GetService<ILogger<App>>()!;

        public static IHostBuilder CreateHostBuilder()
            => Host.CreateDefaultBuilder()
                .ConfigureContainer<IServiceCollection>(collection =>
                {
                    Startup.ConfigureServices(collection);
                    Ioc.Default.ConfigureServices(collection.BuildServiceProvider());
                });

        public App()
        {
            var dataDirectory = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "Data");
            dataDirectory = $"\"{dataDirectory}\"";
            var result = Core.ApplicationSetup.InitPostgresqlAsync(dataDirectory).GetAwaiter().GetResult();
            //AppCenter.Start("1f00432d-26a9-4bd8-86f3-552be7829da0",
            //       typeof(Analytics), typeof(Crashes));
            InitializeComponent();
            UnhandledException += App_UnhandledException;

            ServiceHost = CreateHostBuilder().Build();

            ServiceHost.Start();

            if (Args is not null)
            {
                ActService.ActivateAsync(Args);
            }
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Logger.LogError(e.Exception, e.Message);

            e.Handled = true;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Args = args;
            base.OnLaunched(args);
        }
    }
}
