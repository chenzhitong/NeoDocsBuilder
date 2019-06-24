using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
            Console.ReadLine();
        }

        static string Catalog;
        static MatchCollection CatalogLinks;
        static readonly List<string> AllFiles = new List<string>();
        static void Run(string origin, string destination, string template)
        {
            var files = Directory.GetFiles(origin);
            Catalog += "\r\n<ul>";
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
                var (title, content, sideNav) = Convert(Parse(file));
                Build(destPath, content, title, sideNav, depth, template);
                Catalog += $"<li><a href='{destPath.Replace("\\", "/")}' data-path='{Path.GetFileName(file).Replace(".md", "")}'>{title}</a></li>";
            }
            var dirs = Directory.GetDirectories(origin);
            foreach (var dir in dirs)
            {
                Catalog += "\r\n<li>";
                var dirName = dir.Split("\\").Reverse().ToList()[0];
                if (Config.FolderJson != null)
                {
                    var newName = Config.FolderJson["rename"][dirName]?.ToString();
                    var isHidden = false;
                    foreach (var item in Config.FolderJson["hidden"])
                    {
                        if (item.ToString() == dirName)
                        {
                            isHidden = true;
                            break;
                        }
                    }
                    if (isHidden) continue;
                    if (string.IsNullOrEmpty(newName))
                    {
                        Catalog += $"<span data-icon='+' data-path='{dirName}'>{dirName}</span>";
                    }
                    else
                    {
                        Catalog += $"<span data-icon='+' data-path='{dirName}'>{newName}</span>";
                    }
                }
                else
                {
                    Catalog += $"<span data-icon='+' data-path='{dirName}'>{dirName}</span>";
                }
                Run(dir, destination, template);
                Catalog += "\r\n</li>";
            }
            Catalog += "\r\n</ul>";
        }
        static MarkdownDocument Parse(string name)
        {
            MarkdownDocument document = new MarkdownDocument();
            document.Parse(File.ReadAllText(name));
            return document;
        }
        static (string title, string content, string sideNav) Convert(MarkdownDocument document)
        {
            var sideNav = string.Empty;
            string title = null;
            var content = string.Empty;
            sideNav += "\r\n<ul>";
            foreach (var element in document.Blocks)
            {
                if (element.Type == MarkdownBlockType.Header)
                {
                    var header = (element as HeaderBlock);
                    if (header.HeaderLevel > 3)
                        continue;
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(header.ToHtml());
                    var headerText = xml.InnerText;
                    title = title ?? headerText;
                    var hash = headerText.ToAnchorPoint();
                    sideNav += $"<li><a class='side-nav-{header.HeaderLevel}' onclick='highLight(\"{hash}\")' href='{hash}'>{headerText}</a></li>";
                }
                content += element.ToHtml();
            }
            sideNav += "\r\n</ul>";
            return (title.Trim(), content.Trim(), sideNav.Trim());
        }

        static void Build(string name, string content, string title, string sideNav, int depth, string template)
        {
            var path = Path.Combine(name);
            var depthStr = string.Empty;
            for (int i = 0; i < depth; i++)
            {
                depthStr += "../";
            }
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine(File.ReadAllText(Path.Combine(template, "index.html")).Replace("{title}", title).Replace("{sideNav}", sideNav).Replace("{body}", content).Replace("{depth}", depthStr).Replace("{img_depth}", depthStr == "" ? "." : depthStr));
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
