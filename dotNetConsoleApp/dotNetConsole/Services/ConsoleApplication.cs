using dotNetConsole.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Directory = System.IO.Directory;

namespace dotNetConsole.Services
{
    class ConsoleApplication
    {
        private readonly ICustomer _customer;
        private readonly IAuthenticationConfig _authConfig;
        //private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAPIClient _apiClient;
        private ConfigurationBuilder baseBuilder;

        //private ProtectedApiCallHelper apiCaller;
        //private HttpResponseMessage response;

        public IConfiguration _config { get; private set; }

        public ConsoleApplication(ICustomer customer, IAuthenticationConfig authConfig, IAPIClient apiClient)
        {
            _customer = customer;
            _authConfig = authConfig;
            //_httpClientFactory = httpClientFactory;
            _apiClient = apiClient;
        }

        public async Task Run()
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

            AuthenticationResult result = await AuthenticateSvc();

            _customer.CreateCustomer("Don");
            Console.WriteLine($"New Customer Name: {_customer.CustomerName}");
            Log.Logger.Information($"{_config["hostUrl"]}");
            Console.WriteLine($"Token: {result.AccessToken}");


            // await _apiClient.GetMoviesWithHttpClientFromFactory();
            await _apiClient.GetUsers(result, Display);
            //await GetMoviesWithHttpClientFromFactory();
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

        private async Task<AuthenticationResult> AuthenticateSvc()
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

            //    //HttpClient httpClient = new HttpClient();
            //    //HttpResponseMessage response = new HttpResponseMessage();
            //    if (!string.IsNullOrEmpty(result.AccessToken))
            //    {
            //        var webApiUrl = $"{config.ApiUrl}v1.0/users";
            //        var defaultRequestHeaders = _httpClient.DefaultRequestHeaders;
            //        if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
            //        {
            //            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //        }
            //        defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);

            //        var response = await _httpClient.GetAsync(webApiUrl);


            //        //if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
            //        //{
            //        //    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //        //}
            //        //defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);
            //        //var webApiUrl = $"{config.ApiUrl}v1.0/users";
            //        //try
            //        //{
            //        //    response = await httpClient.GetAsync(webApiUrl);

            //        //}
            //        //catch (Exception ex)
            //        //{

            //        //    throw;
            //        //}

            //        if (response.IsSuccessStatusCode)
            //        {
            //            string json = await response.Content.ReadAsStringAsync();
            //            JObject result1 = JsonConvert.DeserializeObject(json) as JObject;
            //            Console.ForegroundColor = ConsoleColor.Gray;
            //            Display(result1);
            //        }
            //        else
            //        {
            //            Console.ForegroundColor = ConsoleColor.Red;
            //            Console.WriteLine($"Failed to call the Web Api: {response.StatusCode}");
            //            string content = await response.Content.ReadAsStringAsync();

            //            // Note that if you got reponse.Code == 403 and reponse.content.code == "Authorization_RequestDenied"
            //            // this is because the tenant admin as not granted consent for the application to call the Web API
            //            Console.WriteLine($"Content: {content}");
            //        }
            //        Console.ResetColor();

            //    }
            //    //apiCaller = new ProtectedApiCallHelper(httpClient);
            //    //await apiCaller.CallWebApiAndProcessResultASync($"{config.ApiUrl}v1.0/users", result.AccessToken, Display);
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

        //public async Task GetMoviesWithHttpClientFromFactory()
        //{
        //    var httpClient = _httpClientFactory.CreateClient();

        //    var request = new HttpRequestMessage(
        //        HttpMethod.Get,
        //        "http://localhost:57863/api/movies");
        //    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //    //var movies = stream.ReadAndDeserializeFromJson<List<Movie>>();
        //    using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
        //    {
        //        var stream = await response.Content.ReadAsStreamAsync();
        //        using (var streamReader = new StreamReader(stream))
        //        {
        //            using (var jsonTextReader = new JsonTextReader(streamReader))
        //            {
        //                var jsonSerializer = new JsonSerializer();
        //                var users = jsonSerializer.Deserialize<List<User>>(jsonTextReader);
        //            }
        //        }
        //    }
        //}
    }
}
