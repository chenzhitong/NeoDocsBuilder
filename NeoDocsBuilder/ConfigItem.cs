using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NeoDocsBuilder
{
    public class ConfigItem
    {
        public string Origin;
        public string Destination;
        public string Template;
        public string Catalog;
        public string Git;
        public string WebRoot;
        public JObject FolderJson;
        public ConfigItem(JToken json)
        {
            Origin = json["origin"].ToString();
            Destination = json["destination"].ToString();
            Template = json["template"].ToString();
            Catalog = json["catalog"].ToString();
            Git = json["git"].ToString();
            WebRoot = json["webRoot"].ToString();
            var jsonPath = Path.Combine(Origin, "folder.json");
            if (!File.Exists(jsonPath))
            {
                FolderJson = null;
            }
            else
            {
                FolderJson = JObject.Parse(File.ReadAllText(jsonPath));
            }
        }
    }
}
