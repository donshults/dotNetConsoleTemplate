using dotNetConsole.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace dotNetConsole.Services
{
    public class O365SiteServices
    {
        private GraphServiceClient _graphClient;
        private ILogger _logger;
        private string _listId;
        private string _siteUrl;
        private string _siteId;
        public SiteModel SiteModel { get; set; }

        public O365SiteServices(GraphServiceClient graphClient, string siteUrl, ILogger logger)
        {
            _graphClient = graphClient;
            _logger = logger;
            _siteUrl = siteUrl;
            GetBaseSiteInfo(_siteUrl) ;
        }

             private void GetBaseSiteInfo(string _siteUrl)
        {
            SiteModel  = GetRootSiteInfo(_siteUrl).Result;
        }
        public async Task<SiteModel> GetRootSiteInfo(string siteUrl)
        {
            var siteurl = new Uri(siteUrl);
            var rootSite = new SiteModel();
            try
            {
                var tempPath = siteurl.AbsolutePath;
                var tempHost = $"https://{siteurl.Host}";
                var site = await _graphClient.Sites.GetByPath(tempPath, tempHost)
                    .Request()
                    .GetAsync();
                //rootSite 
                rootSite.DisplayName = site.DisplayName;
                rootSite.CreatedDateTime = Utilities.ConvertFromDateTimeOffset((DateTimeOffset)site.CreatedDateTime);
                rootSite.LastModifiedDateTime = Utilities.ConvertFromDateTimeOffset((DateTimeOffset)site.LastModifiedDateTime);
                rootSite.Id = site.Id;
                rootSite.WebUrl = site.WebUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Trying to get Root Site Info {ex.Message}");
            }

            return rootSite;
        }

    }
}
