using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NeoDocsBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            Files.CopyDirectory(Config.Template, Config.Destination);
            Files.CopyDirectory(Config.Origin, Config.Destination);
            Files.CopyDirectoryOnly(Config.Origin, Config.Destination);
            Run(Config.Origin, Config.Destination, Config.Template);
            BuildCatalog(Config.Destination);
        }

        static string Catalog;
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
                var filePathWithoutOrigin = string.Join("\\", split.Skip(1)).Replace(".md", ".html").Replace("\\","/");
                var (title, content, sideNav) = Convert(Parse(file));
                Build(Path.Combine(destination, filePathWithoutOrigin), content, title, sideNav, depth, template);
                var up = "../../../../../../../../../../";
                Catalog += $"<li><a href='{up}{filePathWithoutOrigin}' data-path='{Path.GetFileName(file).Replace(".md", "")}'>{title}</a></li>";
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
                    var str = header.ToString().Trim(' ', '*');
                    var hash = str.ToAnchorPoint();
                    sideNav += $"<li><a class='side-nav-{header.HeaderLevel}' onclick='highLight(\"{hash}\")' href='{hash}'>{str}</a></li>";
                    title = title??element.ToString();
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
                sw.WriteLine(File.ReadAllText(Path.Combine(template, "index.html")).Replace("{title}", title).Replace("{sideNav}", sideNav).Replace("{depth}", depthStr).Replace("{body}", content));
                Console.WriteLine($"build: {name}");
            }
        }

        private static void BuildCatalog(string path)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                if (Path.GetExtension(file) != ".html")
                    continue;
                var html = File.ReadAllText(file).Replace("{catalog}", Catalog);
                using (StreamWriter sw = new StreamWriter(file))
                {
                    sw.WriteLine(html);
                    Console.WriteLine($"catalog: {file}");
                }
            }
            Directory.GetDirectories(path).ToList().ForEach(
                p => BuildCatalog(p)
            );
        }
    }
}
