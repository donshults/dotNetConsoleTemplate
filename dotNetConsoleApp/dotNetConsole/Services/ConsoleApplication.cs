﻿using dotNetConsole.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Directory = System.IO.Directory;

namespace dotNetConsole.Services
{
    class ConsoleApplication
    {
        private readonly ICustomer _customer;
        private readonly IAuthenticationConfig _authConfig;
        private ConfigurationBuilder baseBuilder;

        public IConfiguration _config { get; private set; }

        public ConsoleApplication(ICustomer customer, IAuthenticationConfig authConfig)
        {
            _customer = customer;
            _authConfig = authConfig;
        }

        public void Run()
        {
            _customer.CreateCustomer("Hello World");
            Console.WriteLine($"This is the customer Name: {_customer.CustomerName}");

            baseBuilder = new ConfigurationBuilder();
            _config = BuildConfig(baseBuilder);
            //Setup Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(baseBuilder.Build())
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information($"Application Starting");

            AuthenticationResult result = AuthenticateSvc();

            Log.Logger.Information($"{_config["hostUrl"]}");
        }

        IConfiguration BuildConfig(ConfigurationBuilder builder)
        {
            var config = builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger.Information($"Appsettings: {config["hostUrl"]}");
            return config;
        }

        private AuthenticationResult AuthenticateSvc()
        {
            AuthenticationConfig config = _authConfig.ReadFromJsonFile("appsettings.json");

            bool isUseringClientSecret = AppUsesClientSecret(config);
            // Even if this is a console application here, a daemon application is a confidential client application
            IConfidentialClientApplication app;

            if (isUseringClientSecret)
            {
                app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                    .WithClientSecret(config.ClientSecret)
                    .WithAuthority(new Uri(config.Authority))
                    .Build();
            }
            else
            {
                X509Certificate2 certificate = ReadCertificate(config.CertificateName);
                app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                    .WithCertificate(certificate)
                    .WithAuthority(new Uri(config.Authority))
                    .Build();
            }

            string[] scopes = new string[] { $"{config.ApiUrl}.default" };

            AuthenticationResult result = null;
            try
            {
                result = app.AcquireTokenForClient(scopes).ExecuteAsync().Result;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Token acquired");
                Console.ResetColor();
            }
            catch (MsalClientException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                // Invalid scope. The scope has to be of the form "https://resourceurl/.default"
                // Mitigation: change the scope to be as expected
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Scope provided is not supported");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Message: {ex.Message}");
                Console.ResetColor();
            }

            //if (result != null)
            //{
            //    var httpClient = new HttpClient();
            //    var apiCaller = new ProtectedApiCallHelper(httpClient);
            //    await apiCaller.CallWebApiAndProcessResultASync($"{config.ApiUrl}v1.0/users", result.AccessToken, Display);
            //}
            return result;
        }


        // <summary>
        // Checks if the sample is configured for using ClientSecret or Certificate.This method is just for the sake of this sample.
        // You won't need this verification in your production application since you will be authenticating in AAD using one mechanism only.
        // </summary>
        // <param name = "config" > Configuration from appsettings.json</param>
        // <returns></returns>
        private bool AppUsesClientSecret(AuthenticationConfig config)
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

        private X509Certificate2 ReadCertificate(string certificateName)
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

                //Find unexpired certificates.
               X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

                //From the collection of unexpired certificates, find the ones with the correct name.
                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindBySubjectDistinguishedName, certificateName, false);

                //Return the first certificate in the collection, has the right name and is current.
               cert = signingCert.OfType<X509Certificate2>().OrderByDescending(c => c.NotBefore).FirstOrDefault();
            }
            return cert;
        }

        private void Display(JObject result)
        {
            foreach (JProperty child in result.Properties().Where(p => !p.Name.StartsWith("@")))
            {
                Console.WriteLine($"{child.Name} = {child.Value}");
            }
        }
    }
}