using Newtonsoft.Json.Linq;
using System.IO;

namespace NeoDocsBuilder
{
    public class ConfigItem
    {
        public string Origin;
        public string Destination;
        public string Git;
        public string GitRepoPath;
        public JObject FolderJson;
        public ConfigItem(JToken json)
        {
            Origin = json["origin"].ToString();
            Destination = json["destination"].ToString();
            Git = json["git"].ToString();
            GitRepoPath = json["gitRepoPath"]?.ToString();
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
