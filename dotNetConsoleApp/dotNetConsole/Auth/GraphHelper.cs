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

        static async Task GetMyInfo(GraphServiceClient graphClient, ILogger logger)
        {
            var user = await graphClient.Me
                .Request()
                .GetAsync();
            logger.LogInformation($"Name: {user.DisplayName}");
        }
    }
}
