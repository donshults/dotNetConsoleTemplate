using System;
using System.Collections.Generic;
using System.Text;

namespace dotNetConsole.Models
{
    public class SiteModel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string WebUrl { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public bool IsSiteCollection { get; set; }
    }
}
