using dotNetConsole.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNetConsole.Services
{
    public class O365ListServices
    {
        private GraphServiceClient _graphClient;
        private ILogger _logger;
        private string _siteName;
        private string _listId;
        private string _siteUrl;
        private string _siteId;

        public O365ListServices(GraphServiceClient graphClient, string siteUrl, ILogger logger)
        {
            _graphClient = graphClient;
            _logger = logger;
            _siteUrl = siteUrl;
            _siteId = GetSiteId(_siteUrl).Result;
        }

        public async Task<SiteModel> GetRootSiteInfo(string sitePath, string siteHost)
        {
            var rootSite = new SiteModel();
            try
            {
                var site = await _graphClient.Sites.GetByPath(sitePath, siteHost)
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
                _logger.LogError($"Error Trying to get Root Site Info");
            }

            return rootSite;
        }

        public async Task<string> GetListId(string listName, string siteId)
        {
            List<ListModel> newLists = new List<ListModel>();
            ListModel list = new ListModel();
            string listId = string.Empty;

            newLists = await GetListofLists(siteId, newLists);
            list = newLists.SingleOrDefault(l => l.Name.ToLower() == listName.ToLower());
            return list.Id;
        }

        public async Task<string> GetSiteId(string siteUrl)
        { 
            var parsedUrl = new Uri(siteUrl);
            var siteInfo = new SiteModel();
            try
            {
                var site = await _graphClient.Sites.GetByPath(parsedUrl.LocalPath, parsedUrl.Host)
                    .Request()
                    .GetAsync();
                siteInfo.CreatedDateTime = Utilities.ConvertFromDateTimeOffset((DateTimeOffset)site.CreatedDateTime);
                siteInfo.LastModifiedDateTime = Utilities.ConvertFromDateTimeOffset((DateTimeOffset)site.LastModifiedDateTime);
                siteInfo.Description = site.Description;
                siteInfo.DisplayName = site.DisplayName;
                siteInfo.Id = site.Id;
                siteInfo.IsSiteCollection = site.SiteCollection == null ? false : true;
                siteInfo.Name = site.Name;
                siteInfo.WebUrl = site.WebUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Trying to get Site Info {ex}");
            }
            return siteInfo.Id;
        }

        public async Task<List<ListModel>> GetLists(string siteId)
        {
            List<ListModel> newLists = new List<ListModel>();
            newLists = await GetListofLists(siteId, newLists);
            return newLists;
        }

        private async Task<List<ListModel>> GetListofLists(string siteId, List<ListModel> newLists)
        {
            try
            {
                var listPages = await _graphClient.Sites[siteId].Lists
                    .Request()
                    .GetAsync();

                if (listPages.NextPageRequest == null)
                {
                    if (listPages.Count == 0)
                    {
                        _logger.LogError($"Can't find any lists on this site");
                    }
                    else
                    {
                        newLists = ProcessLists(newLists, listPages);
                    }
                }
                else
                {
                    newLists = ProcessLists(newLists, listPages);
                    do
                    {
                        listPages = await listPages.NextPageRequest.GetAsync();
                        newLists = ProcessLists(newLists, listPages);
                    } while (listPages.NextPageRequest != null);
                }
            }
            catch (Exception)
            {
                _logger.LogError($"Error getting Lists from {siteId}");
            }
            return newLists;
        }

        public async Task<ListModel> GetListByName(string siteId, string listName)
        {
            var newLists = await GetLists(siteId);
            var newList = new ListModel();

            foreach (var list in newLists)
            {
                if (list.Name.ToLower() == listName.ToLower())
                {
                    newList.Description = list.Description;
                    newList.Id = list.Id;
                    newList.Name = list.Name;
                    newList.WebUrl = list.WebUrl;
                    newList.CreatedDateTime = Utilities.ConvertFromDateTimeOffset((DateTimeOffset)list.CreatedDateTime);
                    newList.LastModifiedDateTime = Utilities.ConvertFromDateTimeOffset((DateTimeOffset)list.LastModifiedDateTime);
                    return newList;
                }
            }
            return newList;
        }

        private List<ListModel> ProcessLists(List<ListModel> newLists, ISiteListsCollectionPage listPages)
        {
            var lists = newLists;
            foreach (var list in listPages)
            {
                var newList = new ListModel()
                {
                    Id = list.Id,
                    CreatedDateTime = Utilities.ConvertFromDateTimeOffset((DateTimeOffset)list.CreatedDateTime),
                    LastModifiedDateTime = Utilities.ConvertFromDateTimeOffset((DateTimeOffset)list.LastModifiedDateTime),
                    Description = list.Description,
                    Name = list.Name,
                    WebUrl = list.WebUrl
                };
                lists.Add(newList);
            }
            return lists;
        }
    }
}
