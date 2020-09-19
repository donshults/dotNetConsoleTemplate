using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace dotNetConsole.Auth
{
    public class GraphHelper
    {
        public GraphServiceClient GraphClient { get; set; } = null;
        public IAuthenticationProvider AuthProvider { get; set; } = null;
        public string AccessToken { get; set; } = string.Empty;

        private readonly string _appId;
        private readonly string _tenantId;
        private readonly string _clientSecret;
        private readonly string[] _scopes;
        private readonly ILogger logger;

        public GraphHelper(string appId, string tenantId, string clientSecret, string[] scopes, ILogger logger)
        {
            if (!string.IsNullOrEmpty(appId) || !string.IsNullOrEmpty(tenantId) || !string.IsNullOrEmpty(clientSecret) || scopes.Length == 0)
            {
                _appId = appId;
                _tenantId = tenantId;
                _clientSecret = clientSecret;
                //_scopes = scopes;
                AuthProvider = CreateAuthenticationProvider(logger);
                Initialize(AuthProvider);
            }
            else
            {
                logger.LogInformation($"Missing Config Values");
                return;
            }

            this.logger = logger;
        }

        public IAuthenticationProvider CreateAuthenticationProvider(ILogger logger)
        {
            AuthProvider = null;
            try
            {
                var accessToken = string.Empty;
                AuthProvider = new ClientCredentialsProvider(_appId, _tenantId, _clientSecret, _scopes, logger, out accessToken);
                AccessToken = accessToken;
            }
            catch (Exception ex)
            {
                logger.LogError($"CreateAutenticationProvider Error: {ex.Message}");
                return AuthProvider;
            }
            return AuthProvider;
        }

        public void Initialize(IAuthenticationProvider authProvider)
        {
            try
            {
                GraphClient = new GraphServiceClient(authProvider);
            }
            catch (Exception ex)
            {
                logger.LogError($"CreateAutenticationProvider Error: {ex.Message}");
            }
        }

        static string FormatDateTimeTimeZone(Microsoft.Graph.DateTimeTimeZone value)
        {
            // Get the timezone specified in the Graph value
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(value.TimeZone);
            // Parse the date/time string from Graph into a DateTime
            var dateTime = DateTime.Parse(value.DateTime);

            // Create a DateTimeOffset in the specific timezone indicated by Graph
            var dateTimeWithTZ = new DateTimeOffset(dateTime, timeZone.BaseUtcOffset)
                .ToLocalTime();

            return dateTimeWithTZ.ToString("g");
        }
    }

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
