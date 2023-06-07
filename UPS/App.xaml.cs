using Prism.DryIoc;
using Prism.Ioc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using UPS.Services;
using UPS.ViewModels;

namespace UPS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.File("log.txt")
            .CreateLogger();

            containerRegistry.RegisterInstance<ILogger>(Log.Logger);
            containerRegistry.Register<IMyUserService, MyUserService>();
            containerRegistry.Register<MainWindow>();
            containerRegistry.Register<MyUserAdd>();
            containerRegistry.RegisterSingleton<MainWindowViewModel>();
            containerRegistry.Register<MyUserUpdate>();
            containerRegistry.Register<SearchExportWindow>();

            // Create and configure the HttpClient instance
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://gorest.co.in/public/v2/");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer 0bf7fb56e6a27cbcadc402fc2fce8e3aa9ac2b40d4190698eb4e8df9284e2023");
            containerRegistry.RegisterInstance(httpClient);

        }

    }
}
