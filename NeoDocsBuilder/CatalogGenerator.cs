using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NeoDocsBuilder
{
    internal class CatalogGenerator
    {
        static int depth = 0;

        public static string ConvertFromCatalogJson(string path)
        {
            var dirs = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path, "*.md");
            var currentFlagList = new List<Flag>();
            foreach (var item in dirs)
            {
                var flag = CatalogDeserialize(item);
                if (flag != null) 
                    currentFlagList.Add(flag);
            }
            foreach (var item in files)
            {
                var flag = YamlDeserialize(item);
                if (flag != null)
                    currentFlagList.Add(flag);
            }
            currentFlagList = currentFlagList.OrderBy(p => p.Position).ToList();

            var nav = $"\r\n{AddSpace(depth)}<nav class='nav nav-pills flex-column ml-3'>";
            var navend = $"\r\n{AddSpace(depth)}</nav>";
            var catalog = "";
            catalog += nav;
            depth++;
            foreach (var item in currentFlagList)
            {
                if (!item.Link.EndsWith("md"))
                {
                    catalog += $"\r\n{AddSpace(depth)}<span class='ml-0 my-1 nav-link'>{item.Label}<i class='fas fa-chevron-right'></i></span>";
                    catalog += ConvertFromCatalogJson(item.Link);
                }
                else
                {
                    catalog += $"\r\n{AddSpace(depth)}<a class='ml-0 my-1 nav-link' href='/{item.Link.Replace("\\", "/").Replace(".md", ".html")}'>{item.Label}</a>";
                }
            }
            depth--;
            catalog += navend;
            return catalog;
        }

        public static string AddSpace(int depth)
        {
            var output = "";
            for (int i = 0; i < depth; i++)
            {
                output += "  ";
            }
            return output;
        }

        public static Flag YamlDeserialize(string path)
        {
            var md = File.ReadAllLines(path);
            var label = "";
            var position = 1000;
            for (int i = 1; i < md.Length; i++)
            {
                if (md[i].StartsWith("sidebar_label"))
                    label = md[i].Split(':')[1].Trim(' ').Trim('\'');
                if (md[i].StartsWith("sidebar_position"))
                    position = Convert.ToInt32(md[i].Split(':')[1].Trim(' ').Trim('\''));
                if (md[i].StartsWith("---"))
                    break;
            }
            if(string.IsNullOrEmpty(label))
                label = Path.GetFileNameWithoutExtension(path);
            return new Flag { Label = label, Position = position, Link = path };
        }

        public static Flag CatalogDeserialize(string path)
        {
            var jsonPath = Path.Combine(path, "_category_.json");
            if (!File.Exists(jsonPath)) return null;
            var json = JObject.Parse(File.ReadAllText(jsonPath));
            var label = (string)json["label"];
            var position = (int)json["position"];

            return new Flag { Label = label, Position = position, Link = path };
        }
    }

    public class Flag
    {
        public string Label;

        public int Position;

        public string Link;
    }
}
