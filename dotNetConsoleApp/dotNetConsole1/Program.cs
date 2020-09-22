using dotNetConsole1.Auth;
using dotNetConsole1.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace dotNetConsole1
{
    class Program
    {
        private static AuthenticationConfig _authConfig;

        static void Main(string[] args)
        {
            _authConfig = new AuthenticationConfig();

            Console.WriteLine("Demo Console with DI");
            string testvalue = "Hello";
            //Config Read for Authentication
            var config = _authConfig.ReadFromJsonFile("appsettings.json");
            
            //Config Read for Normal
            var baseBuilder = new ConfigurationBuilder();
            BuildConfig(baseBuilder);

            //Setup Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(baseBuilder.Build())
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information($"Application Starting {config.ClientId}");

            //Setup DI
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    //services.AddTransient<IAuthenticationConfig, AuthenticationConfig>();
                    services.AddTransient<ICustomer, Customer>();
                })
                .UseSerilog()
                .Build();

            //var authResult = await AuthenticateSvcAsync();

            //var svc1 = ActivatorUtilities.CreateInstance<AuthenticationConfig>(host.Services);
            var customer = ActivatorUtilities.CreateInstance<Customer>(host.Services);
            customer.CreateCustomer("Don");

            Log.Logger.Information($"My Customer Name:{0}", customer.CustomerName);
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
