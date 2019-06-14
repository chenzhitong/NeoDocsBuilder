using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NeoDocsBuilder
{
    public static class Config
    {
        private static bool? lazyload;
        private static string origin;
        private static string destination;
        private static string template;
        private static JObject folder_json;
        public static bool? Lazyload
        {
            get {
                if (lazyload == null)
                    Refresh();
                return lazyload;
            }
        }
        public static string Origin
        {
            get
            {
                if (origin == null)
                    Refresh();
                return origin;
            }
        }
        public static string Destination
        {
            get
            {
                if (destination == null)
                    Refresh();
                return destination;
            }
        }
        public static string Template
        {
            get
            {
                if (template == null)
                    Refresh();
                return template;
            }
        }
        public static JObject FolderJson
        {
            get
            {
                if (folder_json == null)
                    Refresh();
                return folder_json;
            }
        }
        private static void Refresh()
        {
            var json = JObject.Parse(File.ReadAllText("config.json"))["ApplicationConfiguration"];
            lazyload = bool.Parse(json["lazyload"].ToString());
            origin = json["origin"].ToString();
            destination = json["destination"].ToString();
            template = json["template"].ToString();
            var jsonPath = Path.Combine(origin, "folder.json");
            if (!File.Exists(jsonPath))
            {
                folder_json = null;
            }
            else
            {
                folder_json = JObject.Parse(File.ReadAllText(jsonPath));
            }
        }
    }
}
