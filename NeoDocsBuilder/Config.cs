using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NeoDocsBuilder
{
    public static class Config
    {
        public static readonly List<ConfigItem> ConfigList = new List<ConfigItem>();
        private static string _configFile = "config.json";
        public static string ConfigFile
        {
            get { return _configFile; }
            set { _configFile = value; Refresh(); }
        }

        public static void Refresh()
        {
            var json = JObject.Parse(File.ReadAllText(ConfigFile))["ApplicationConfiguration"];
            ConfigList.Clear();
            foreach (var item in json)
            {
                ConfigList.Add(new ConfigItem(item));
            }
        }
    }
}
