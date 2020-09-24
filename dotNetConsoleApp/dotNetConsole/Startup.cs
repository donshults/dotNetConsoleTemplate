using dotNetConsole.Auth;
using dotNetConsole.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dotNetConsole
{
    public static class Startup
    {
        private static AuthenticationConfig _authConfig;
        private static ServiceProvider _serviceProvider { get; set; }
        public static void ConfigureService(ServiceCollection services)
        {

            services.AddLogging();
            services.AddHttpClient("MyHttpClient", client =>
            {
                client.BaseAddress = new Uri("http://localhost:57863");
                client.Timeout = new TimeSpan(0, 0, 30);
                client.DefaultRequestHeaders.Clear();
            });
            services.AddSingleton<IAPIClient, APIClient>();
            services.AddSingleton<ICustomer, Customer>();
            services.AddSingleton<IAuthenticationConfig, AuthenticationConfig>();
            services.AddSingleton<ConsoleApplication>();
            _serviceProvider = services.BuildServiceProvider(true);
        }
        public static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }
        //Build a IRoot Configuration Object
        public static IConfiguration BuildConfig(ConfigurationBuilder builder)
        {
            var config = builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            return config;
        }
    }
}
