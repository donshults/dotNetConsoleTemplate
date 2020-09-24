using dotNetConsole.Auth;
using dotNetConsole.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Directory = System.IO.Directory;

namespace dotNetConsole
{
    class Program
    {
        private static AuthenticationConfig _authConfig;

        public static ServiceProvider _serviceProvider { get; private set; }

        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureService(services);

            var serviceProvider = services.BuildServiceProvider();

            Console.WriteLine("Demo Console with DI");
            string testvalue = "Hello";

            try
            {
                await serviceProvider.GetService<ConsoleApplication>().Run();
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetService<ILogger<Program>>();
                throw;
            }

           // IServiceScope scope = _serviceProvider.CreateScope();
           // scope.ServiceProvider.GetRequiredService<ConsoleApplication>().Run();
           // DisposeServices();
            
        }

        private static void ConfigureService(ServiceCollection services)
        {
            
            services.AddLogging();
            services.AddHttpClient("MoviesClient", client =>
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

        //private static void RegisterServices()
        //{
        //    var services = new ServiceCollection();
        //    services.AddLogging();
        //    services.AddHttpClient("MoviesClient", client =>
        //    {
        //        client.BaseAddress = new Uri("http://localhost:57863");
        //        client.Timeout = new TimeSpan(0, 0, 30);
        //        client.DefaultRequestHeaders.Clear();
        //    });
        //    //services.AddSingleton<IAPIClient, APIClient>();
        //    services.AddSingleton<ICustomer, Customer>();
        //    services.AddSingleton<IAuthenticationConfig, AuthenticationConfig>();
        //    services.AddSingleton<ConsoleApplication>();
        //    _serviceProvider = services.BuildServiceProvider(true);
        //}

        private static void DisposeServices()
        {
            if(_serviceProvider == null)
            {
                return;
            }
            if(_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        /// <summary>
        /// Display the result of the Web API call
        /// </summary>
        /// <param name="result">Object to display</param>
        private static void Display(JObject result)
        {
            foreach (JProperty child in result.Properties().Where(p => !p.Name.StartsWith("@")))
            {
                Console.WriteLine($"{child.Name} = {child.Value}");
            }
        }

        /// <summary>
        /// Checks if the sample is configured for using ClientSecret or Certificate. This method is just for the sake of this sample.
        /// You won't need this verification in your production application since you will be authenticating in AAD using one mechanism only.
        /// </summary>
        /// <param name="config">Configuration from appsettings.json</param>
        /// <returns></returns>
        private static bool AppUsesClientSecret(AuthenticationConfig config)
        {
            string clientSecretPlaceholderValue = "[Enter here a client secret for your application]";
            string certificatePlaceholderValue = "[Or instead of client secret: Enter here the name of a certificate (from the user cert store) as registered with your application]";

            if (!String.IsNullOrWhiteSpace(config.ClientSecret) && config.ClientSecret != clientSecretPlaceholderValue)
            {
                return true;
            }

            else if (!String.IsNullOrWhiteSpace(config.CertificateName) && config.CertificateName != certificatePlaceholderValue)
            {
                return false;
            }

            else
                throw new Exception("You must choose between using client secret or certificate. Please update appsettings.json file.");
        }
        private static X509Certificate2 ReadCertificate(string certificateName)
        {
            if (string.IsNullOrWhiteSpace(certificateName))
            {
                throw new ArgumentException("certificateName should not be empty. Please set the CertificateName setting in the appsettings.json", "certificateName");
            }
            X509Certificate2 cert = null;

            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = store.Certificates;

                // Find unexpired certificates.
                X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

                // From the collection of unexpired certificates, find the ones with the correct name.
                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindBySubjectDistinguishedName, certificateName, false);

                // Return the first certificate in the collection, has the right name and is current.
                cert = signingCert.OfType<X509Certificate2>().OrderByDescending(c => c.NotBefore).FirstOrDefault();
            }
            return cert;
        }
    }
}
