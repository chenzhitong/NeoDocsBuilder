using Microsoft.Toolkit.Parsers.Markdown;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace NeoDocsBuilder
{
    internal class CatalogGenerator
    {
        static int depth = 0;
        static string[] blockList = ["assets"];

        public static string ConvertFromCatalogJson(string path)
        {
            var currentFlagList = Directory.GetDirectories(path).Where(p => !blockList.Contains(Path.GetFileName(p))).Select(CatalogDeserialize).Where(flag => flag != null)
            .Concat(Directory.GetFiles(path, "*.md").Select(YamlDeserialize).Where(flag => flag != null))
            .OrderBy(flag => flag.Position).ToList();

            var catalog = $"\r\n{new string(' ', depth * 2)}<nav class='nav nav-pills flex-column ml-3'>";
            depth++;
            catalog += string.Join("", currentFlagList.Select(item =>
            {
                var space = new string(' ', depth * 2);
                return item.Link.EndsWith("md")
                    ? $"\r\n{space}<a class='ml-0 my-1 nav-link' href='/{item.Link.Replace("\\", "/").Replace(".md", ".html")}'>{item.Label}</a>"
                    : $"\r\n{space}<span class='ml-0 my-1 nav-link'>{item.Label}<i class='fas fa-chevron-right'></i></span>" + ConvertFromCatalogJson(item.Link);
            }));

            depth--;
            catalog += $"\r\n{new string(' ', depth * 2)}</nav>";
            return catalog;
        }

        public static Flag YamlDeserialize(string path)
        {
            string label = null;
            var position = 1000;
            if (path.Contains("instructions.md"))
            { 
            }
            foreach (var item in File.ReadAllLines(path).Skip(1).Take(10))
            {
                if (item.StartsWith("sidebar_label"))
                    label = item.Split(':')[1].Trim(' ').Trim('\'');
                if (item.StartsWith("sidebar_position"))
                    position = Convert.ToInt32(item.Split(':')[1].Trim(' ').Trim('\''));
                if (item.StartsWith("---"))
                    break;
            }
            if (string.IsNullOrEmpty(label))
            {
                MarkdownDocument document = new();
                document.Parse(File.ReadAllText(path));
                foreach (var item in document.Blocks)
                {
                    if (item.Type == MarkdownBlockType.Header)
                    {
                        label = item.ToString();
                        break;
                    }
                }
            }
            label = string.IsNullOrEmpty(label) ? Path.GetFileNameWithoutExtension(path) : label;
            return new Flag { Label = label, Position = position, Link = path };
        }

        public static Flag CatalogDeserialize(string path)
        {
            var jsonPath = Path.Combine(path, "_category_.json");
            if (!File.Exists(jsonPath))
                return new Flag { Label = Path.GetFileName(path), Position = 1000, Link = path };
            var json = JObject.Parse(File.ReadAllText(jsonPath));
            return new Flag { Label = (string)json["label"], Position = (int)json["position"], Link = path };
        }
    }

    public class Flag
    {
        public string Label;

        public int Position;

        public string Link;
    }
}
