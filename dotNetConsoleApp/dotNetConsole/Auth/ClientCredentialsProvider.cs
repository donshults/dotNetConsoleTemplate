using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace dotNetConsole.Auth
{
    public class ClientCredentialsProvider : IAuthenticationProvider
    {
        private IConfidentialClientApplication _msalClient;
        private string[] _scopes;
        private readonly ILogger _log;

        public ClientCredentialsProvider(string appId, string tenantId, string clientSecret, string[] scopes, ILogger log, out string accessToken)
        {
            _scopes = scopes;
            _log = log;

            _msalClient = ConfidentialClientApplicationBuilder
                .Create(appId)
                .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
                .WithClientSecret(clientSecret)
                .Build();
            accessToken = GetAccessToken().Result;
        }

        public async Task<string> GetAccessToken()
        {
            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
            AuthenticationResult result = null;
            try
            {
                result = await _msalClient.AcquireTokenForClient(scopes)
                    .ExecuteAsync();
                return result.AccessToken;
            }
            catch (Exception exception)
            {
                _log.LogError($"Error getting access token: {exception.Message}");
                return null;
            }
        }

        // This is the required function to implement IAuthenticationProvider
        // The Graph SDK will call this function each time it makes a Graph
        // call.
        public async Task AuthenticateRequestAsync(HttpRequestMessage requestMessage)
        {
            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("bearer", await GetAccessToken());
            requestMessage.Headers.Add("Prefer", "HonorNonIndexedQueriesWarningMayFailRandomly");
        }
    }
}
