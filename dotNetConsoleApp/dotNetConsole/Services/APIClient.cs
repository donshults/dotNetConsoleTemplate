using dotNetConsole.Auth;
using dotNetConsole.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dotNetConsole.Services
{
    public class APIClient : IAPIClient
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuthenticationConfig _config;
        private string webAPIUrl;

        public APIClient(IHttpClientFactory httpClientFactory, IAuthenticationConfig authConfig)
        {
            _httpClientFactory = httpClientFactory;
            _config = authConfig;
            //_result = result;
            webAPIUrl = $"{_config.ApiUrl}v1.0/users";
        }
        public async Task GetUsers(AuthenticationResult result, Action<JObject> processResult)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, webAPIUrl);
            var defaultRequestHeaders = httpClient.DefaultRequestHeaders;
            if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);

            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                var json = await response.Content.ReadAsStringAsync();

                JObject newResult = JsonConvert.DeserializeObject(json) as JObject;
                processResult(newResult);             
            }
        }

        public async Task GetMoviesWithHttpClientFromFactory()
        {
            var httpClient = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "http://localhost:57863/api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //var movies = stream.ReadAndDeserializeFromJson<List<Movie>>();
            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                var stream = await response.Content.ReadAsStreamAsync();
                using (var streamReader = new StreamReader(stream))
                {
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        var jsonSerializer = new JsonSerializer();
                        var users = jsonSerializer.Deserialize<List<User>>(jsonTextReader);
                    }
                }
            }
        }

    }

}
