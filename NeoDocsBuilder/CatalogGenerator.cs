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
            path = path.ToLower();
            var label = "";
            var position = 1000;
            var lines = File.ReadAllLines(path).Skip(1).Take(10);
            try
            {
                //sidebar_label 优先
                label = lines.FirstOrDefault(l => l.StartsWith("sidebar_label"))?.Split(':')[1].Trim(' ', '\'');
                //标题其次
                if (string.IsNullOrEmpty(label))
                {
                    var document = new MarkdownDocument();
                    document.Parse(File.ReadAllText(path));
                    label = document.Blocks.FirstOrDefault(p => p.Type == MarkdownBlockType.Header)?.ToString();
                }
                //文件名最次
                if (string.IsNullOrEmpty(label))
                {
                    label = Path.GetFileNameWithoutExtension(path);
                }
                var sidebarPosition = lines.FirstOrDefault(l => l.StartsWith("sidebar_position"))?.Split(':')[1].Trim(' ', '\'');
                if (!string.IsNullOrEmpty(sidebarPosition))
                {
                    position = Convert.ToInt32(sidebarPosition);
                }
            }
            catch (Exception)
            {
            }
            return new Flag { Label = label, Position = position, Link = path };
        }

        public static Flag CatalogDeserialize(string path)
        {
            path = path.ToLower();
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
