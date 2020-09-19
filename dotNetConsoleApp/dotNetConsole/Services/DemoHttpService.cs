using dotNetConsole.Auth;
using dotNetConsole.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace dotNetConsole.Services
{
    public class DemoHttpService : IIntegrationService
    {
        private static HttpClient _httpClient = new HttpClient();
        private readonly ILogger _logger;
        private IConfiguration _config;
        private string appId;
        private string tenantId;
        private string clientSecret;
        private string[] scopes;
        private string listName;
        private string siteUrl;
        private GraphHelper graphHelper;
        private GraphServiceClient graphClient;
        private object siteId;
        private O365SiteServices siteServices;
        public object rootSiteInfo { get; private set; }

        public DemoHttpService(ILogger<DemoHttpService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            var test = _config["hostUrl"];
            _httpClient.BaseAddress = new Uri(_config.GetValue<string>("hostUrl"));
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
        }

        public async Task Run()
        {
            InitServices(_logger);
            DemoProcess();
            //await GetResource();
            //await GetResourceThroughHttpRequestMessage();
            //await CreateResource();
            //await UpdateResource();
            //await DeleteResource();
        }

        private void DemoProcess()
        {
            siteUrl = "https://precastcorp.sharepoint.com/sites/dsdev";
            _logger.LogInformation($"Staring Main Process ");
            if (!String.IsNullOrEmpty(siteUrl))
            {
                rootSiteInfo = siteServices.GetRootSiteInfo(siteUrl);
                Console.WriteLine($"Site Name: {rootSiteInfo}");
            }
        }

 

        public async Task GetResource()
        {
            _logger.LogInformation($"URL: {_config["hostUrl"]}");
            var response = await _httpClient.GetAsync("api/movies");

            //Handle Errors in the Http Request
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine("The Requested movie cannot be found");
                    return;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //trigger a login flow
                    return;
                }
                response.EnsureSuccessStatusCode();
            }

            var content = await response.Content.ReadAsStringAsync();
            var movies = new List<Movie>();
            if (response.Content.Headers.ContentType.MediaType == "application/json")
            {
                movies = JsonConvert.DeserializeObject<List<Movie>>(content);
            }
            else if (response.Content.Headers.ContentType.MediaType == "application/xml")
            {
                var serializer = new XmlSerializer(typeof(List<Movie>));
                movies = (List<Movie>)serializer.Deserialize(new StringReader(content));
            }
        }

        public async Task GetResourceThroughHttpRequestMessage()
        {
            _logger.LogInformation($"URL: {_config["hostUrl"]}");
            var request = new HttpRequestMessage(HttpMethod.Get, "api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var movies = new List<Movie>();
            movies = JsonConvert.DeserializeObject<List<Movie>>(content);
        }

        public async Task CreateResource()
        {
            var movieToCreate = new MovieForCreation()
            {
                Title = "Reservoir Dogs",
                Description = "After a simple jewelry height goes terribly wrong, the" +
                "surviving criminals begin to suspect that one of them in a police informant.",
                DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
                Genre = "Crime, Drama"
            };

            var serializedMovieToCreate = JsonConvert.SerializeObject(movieToCreate);

            var request = new HttpRequestMessage(HttpMethod.Post, "api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(serializedMovieToCreate);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var createdMovie = JsonConvert.DeserializeObject<Movie>(content);
        }

        private async Task UpdateResource()
        {
            var movieToCreate = new MovieForCreation()
            {
                Title = "Pulp Fiction",
                Description = "The movie with Zed.",
                DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
                Genre = "Crime, Drama"
            };
            var serializedMovieToUpdate = JsonConvert.SerializeObject(movieToCreate);

            var request = new HttpRequestMessage(HttpMethod.Put, "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(serializedMovieToUpdate);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var createdMovie = JsonConvert.DeserializeObject<Movie>(content);
        }

        private async Task DeleteResource()
        {
            var request = new HttpRequestMessage(HttpMethod.Delete,
                "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
        }

        private void InitServices(ILogger logger)
        {
            _logger.LogInformation("Entering InitServices");
            try
            {
                _config = new ConfigurationBuilder()
                    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
                appId = _config["Values:appId"];
                logger.LogInformation($"appId: {appId}");
                tenantId = _config["Values:tenantId"];
                logger.LogInformation($"tenantId: {tenantId}");
                clientSecret = _config["Values:clientSecret"];
                logger.LogInformation($"clientSecret: {clientSecret}");
                scopes = _config.GetSection("Values:scopes").GetChildren().Select(x => x.Value).ToArray();

                if (!string.IsNullOrEmpty(_config["Values:siteUrl"]))
                {
                    siteUrl = _config["Values:siteUrl"];
                    logger.LogInformation($"SiteUrl: {siteUrl}");
                }
                else
                {
                    logger.LogInformation($"Error trying to get siteURL {siteUrl}");
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation($"Error with Building Config: {ex}");
            }

            graphHelper = new GraphHelper(appId, tenantId, clientSecret, scopes, logger);
            graphClient = graphHelper.GraphClient;

            //siteId = listServices.GetSiteId(siteUrl).Result;
            siteServices = new O365SiteServices(graphClient, siteUrl, logger);
        }
    }
}
