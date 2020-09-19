using dotNetConsole.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Net.Http;
using Microsoft.Extensions.Http;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;

namespace dotNetConsole
{
    class Program
    {

        static async Task Main(string[] args)
        {
            //Build Cofiguration
            Console.WriteLine("Demo Console with DI");

            //Create a Host Builder Object to support DI
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(configure => configure.AddConsole() );
                    //services.AddHttpClient();
                    services.AddTransient<DemoHttpService>();
                }).Build();
           
            using (var serviceScope = hostBuilder.Services.CreateScope())
            {

                var services = serviceScope.ServiceProvider;
                try
                {
                    //var demoService = services.GetService<DemoHttpService>();
                    //await demoService.Run();

                    var demoService = services.GetService<DemoHttpService>();
                    var siteService = services.GetService<O365SiteServices>();
                    await demoService.Run();
                    Console.WriteLine("Success");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error Occured: {ex.Message}");
                }
            }
           // Console.ReadKey();
        }
    }
}
