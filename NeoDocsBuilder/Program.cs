using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Web.HttpUtility;
using System.Xml;

namespace NeoDocsBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            var time1 = DateTime.Now;
            Files.CopyDirectory(Config.Template, Config.Destination);
            Files.CopyDirectory(Config.Origin, Config.Destination);
            Files.CopyDirectoryOnly(Config.Origin, Config.Destination);
            Run(Config.Origin, Config.Destination, Config.Template);
            CatalogLinks = new Regex("href='.*?\\.html'").Matches(Catalog);
            BuildCatalog(Config.Destination);

            var time2 = DateTime.Now;
            Console.WriteLine($"{(time2 - time1).TotalSeconds}s");
            //Console.ReadLine();
        }

        static string Catalog;
        static MatchCollection CatalogLinks;
        static readonly List<string> AllFiles = new List<string>();
        static void Run(string origin, string destination, string template)
        {
            var files = Directory.GetFiles(origin);
            Catalog += "\r\n<nav class='nav nav-pills flex-column ml-2'>";
            foreach (var file in files)
            {
                if (Path.GetExtension(file) != ".md")
                    continue;
                var split = file.Split("\\").ToArray();
                if (split.Length < 2)
                    throw new Exception();
                var depth = split.Length - 2;
                var filePathWithoutOrigin = string.Join("\\", split.Skip(1)).Replace(".md", ".html");
                var destPath = Path.Combine(destination, filePathWithoutOrigin);
                //根据二级标题自动折叠
                var collapse = Config.FolderJson != null && Config.FolderJson["collapse"].ToList().Any(p => p.ToString().Equals(string.Join("\\", split.Skip(1)), StringComparison.OrdinalIgnoreCase));
                var (title, content, sideNav) = Convert(Parse(file), collapse);
                Build(destPath, content, title, sideNav, depth, template, collapse);
                Catalog += $"<a class='ml-0 my-1 nav-link' href='{destPath.Replace("\\", "/")}' data-path='{filePathWithoutOrigin.Replace("\\", "/").Replace(".md", "")}'>{title}</a>";
            }
            var dirs = Directory.GetDirectories(origin);
            foreach (var dir in dirs)
            {
                var dirName = dir.Split("\\").Reverse().ToList()[0];
                if (Config.FolderJson != null)
                {
                    var newName = Config.FolderJson["rename"][dirName]?.ToString();
                    if(Config.FolderJson["hidden"].Any(p => p.ToString() == dirName)) continue;
                    Catalog += $"<span class='ml-0 my-1 nav-link' data-icon='+'>{(string.IsNullOrEmpty(newName) ? dirName : newName)}</span>";
                }
                else
                {
                    Catalog += $"<span class='ml-0 my-1 nav-link' data-icon='+'>{dirName}</span>";
                }
                Run(dir, destination, template);
            }
            Catalog += "\r\n</nav>";
        }
        static MarkdownDocument Parse(string name)
        {
            MarkdownDocument document = new MarkdownDocument();
            document.Parse(File.ReadAllText(name));
            return document;
        }
        static (string title, string content, string sideNav) Convert(MarkdownDocument document, bool collapse)
        {
            //文章内的导航
            var sideNav = string.Empty;
            //文章标题（首个标题的文本）
            string title = null;
            //文章正文
            var content = string.Empty;
            bool startCollapse = false;

            var lastHeaderLevel = 0;
            foreach (var element in document.Blocks)
            {
                if (element.Type == MarkdownBlockType.Header)
                {
                    var header = element as HeaderBlock;
                    if (header.HeaderLevel > 3)
                        continue;
                    for (int i = 0; i < header.HeaderLevel - lastHeaderLevel; i++)
                    {
                        sideNav += "\r\n<nav class='nav nav-pills flex-column'>";
                    }
                    for (int i = 0; i < lastHeaderLevel - header.HeaderLevel; i++)
                    {
                        sideNav += "\r\n</nav>";
                    }
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(header.ToHtml());
                    var headerText = xml.InnerText;
                    title = title ?? headerText;
                    var hidden = header.HeaderLevel == 1 ? " d-none" : "";
                    sideNav += $"\r\n<a class='ml-{(header.HeaderLevel - 2) * 2}{hidden} my-1 nav-link' href='{header.ToString().ToAnchorPoint()}' onclick='highLightObj(this)'>{headerText}</a>";

                    lastHeaderLevel = header.HeaderLevel;
                }
                //① 如果 collapse 为 true，则在 h2 下面的所有内容用 <div></div> 包裹起来
                if (collapse && (element as HeaderBlock)?.HeaderLevel == 2 && startCollapse)
                {
                    content += "</div>";
                    startCollapse = false;
                }
                content += element.ToHtml(collapse ? "collapse" : "");
                //② 如果 collapse 为 true，则在 h2 下面的所有内容用 <div></div> 包裹起来
                if (collapse && (element as HeaderBlock)?.HeaderLevel == 2 && !startCollapse)
                {
                    content += "\r\n<div class='div-collapse p-2'>";
                    startCollapse = true;
                }
            }
            //③ 如果 collapse 为 true，则在 h2 下面的所有内容用 <div></div> 包裹起来
            if (collapse && startCollapse)
            {
                content += "</div>";
            }
            for (int i = 0; i < lastHeaderLevel - 0; i++)
            {
                sideNav += "\r\n</nav>";
            }
            return (title.Trim(), content.Trim(), sideNav.Trim());
        }

        static void Build(string name, string content, string title, string sideNav, int depth, string template, bool collapse)
        {
            var path = Path.Combine(name);
            var depthStr = string.Empty;
            for (int i = 0; i < depth; i++)
            {
                depthStr += "../";
            }
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine(File.ReadAllText(Path.Combine(template, "index.html"))
                    .Replace("{title}", title).
                    Replace("{sideNav}", sideNav)
                    .Replace("{body}", content)
                    .Replace("{depth}", depthStr)
                    .Replace("_collapse", collapse.ToString().ToLower()));
                Console.WriteLine($"build: {name}");
            }
        }
        
        private static void BuildCatalog(string path)
        {
            void GetAllFiles(string _path)
            {
                Directory.GetFiles(_path).ToList().ForEach(p => AllFiles.Add(p));
                Directory.GetDirectories(_path).ToList().ForEach(p => GetAllFiles(p));
            };
            GetAllFiles(path);
            Parallel.ForEach(AllFiles, (file) =>
            {
                if (Path.GetExtension(file) != ".html")
                    return;
                ProcessRelativePath(file, Catalog);
            });
        }

        private static void ProcessRelativePath(string file, string catalog)
        {
            foreach (var link in CatalogLinks)
            {
                if (link == null) continue;
                var pathHref = (link as Match).Value;
                var absolute = pathHref.Substring(6, pathHref.Length - 7);
                var relative = Path.GetRelativePath(file, absolute);
                if (relative.StartsWith("..\\"))
                    relative = relative.Remove(0, 3).Replace("\\", "/");
                if (relative == ".")
                    relative = Path.GetFileName(file);
                catalog = catalog.Replace(absolute, relative);
            }
            var html = File.ReadAllText(file).Replace("{catalog}", catalog);
            using (StreamWriter sw = new StreamWriter(file))
            {
                sw.WriteLine(html);
                Console.WriteLine($"catalog: {file}");
            }
        }
    }
}
