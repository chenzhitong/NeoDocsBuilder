using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace NeoDocsBuilder
{
    public static class Config
    {
        public static readonly List<ConfigItem> ConfigList = new List<ConfigItem>();
        private static string _configFile;
        public static string ConfigFile
        {
            get { return _configFile; }
            set { _configFile = value; Refresh(); }
        }

         static void Refresh()
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
