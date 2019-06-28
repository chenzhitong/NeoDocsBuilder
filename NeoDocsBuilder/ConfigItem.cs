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
        public JObject FolderJson;
        public ConfigItem(JToken json)
        {
            Origin = json["origin"].ToString();
            Destination = json["destination"].ToString();
            Template = json["template"].ToString();
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
