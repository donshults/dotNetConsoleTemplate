using DemoLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace dotNetConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //Dependency Injection Template
            using IHost host = CreateHostBuilder(args).Build();
            using var scope = host.Services.CreateScope();

            var services = scope.ServiceProvider;
            try
            {
                services.GetRequiredService<App>().Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error has occurred: { ex.Message }" );
                Console.ReadLine();
            }            
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services
                   .AddTransient<IDemoMessages, DemoMessages>()
                   .AddTransient<App>();
            });
    }
}
