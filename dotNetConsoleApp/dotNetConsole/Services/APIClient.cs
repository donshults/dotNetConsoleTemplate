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
            webAPIUrl = $"{_config.ApiUrl}v1.0/users";
        }
        public async Task GetUsers(AuthenticationResult result, CancellationTokenSource _cancellationTokenSource, Action<JObject> processResult)
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
                //Read as string
                var json = await response.Content.ReadAsStringAsync();
                JObject newResult = JsonConvert.DeserializeObject(json) as JObject;
                processResult(newResult);
                //Read as stream
                response.EnsureSuccessStatusCode();
                //JObject newResult = new JObject();
                //var jsonStream = await response.Content.ReadAsStreamAsync();
                //using (var streamReader = new StreamReader(jsonStream))
                //{
                //    using (var jsonTextReader = new JsonTextReader(streamReader))
                //    {
                //        var jsonSerialilzer = new JsonSerializer();
                //        newResult = jsonSerialilzer.Deserialize(jsonTextReader) as JObject;
                //    }
                //}

                //foreach(var keypair in newResult)
                //{
                //    Console.WriteLine($"Key:{keypair.Key} Value: {keypair.Value}");
                //}

                //processResult(users);
            }
        }

        public async Task GetUsers(AuthenticationResult result, CancellationTokenSource cancellationTokenSource)
        {
            var httpClient = _httpClientFactory.CreateClient("MyHttpClient");
            var request = new HttpRequestMessage(HttpMethod.Get, webAPIUrl);
            var defaultRequestHeaders = httpClient.DefaultRequestHeaders;
            if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);

            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                //Read as stream
                response.EnsureSuccessStatusCode();               
                var jsonStream = await response.Content.ReadAsStreamAsync();
                JObject newResult = new JObject();
                var users = jsonStream.ReadAndDeserializeFromJson<User>();
                Display(users);
            }
        }

        public static void Display(List<User> results)
        {
            foreach (var child in results)
            {
                Console.WriteLine($"{child.DisplayName}");
            }
        }


    }

}
