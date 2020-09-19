using System;
using System.Collections.Generic;
using System.Text;

namespace dotNetConsole.Models
{
    public class ListModel
    {
        public string Id { get; set; }
        public string Name { get; set; } //Name of list
        public string Description { get; set; }
        public string WebUrl { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
    }
}
