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
using System.Threading;

namespace NeoDocsBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var t1 = DateTime.Now;
                //设置配置文件
                Config.ConfigFile = args.Length > 1 ? args[0] : "config.json";

                foreach (var item in Config.ConfigList)
                {
                    Clear();
                    //复制模板到输出目录，包括 CSS、JS、字体、图片等，不包含 MarkDown 文件
                    Files.CopyDirectory(item.Template, item.Destination);
                    //复制源文件夹中的资源文件到输出目录，包括图片等，不包含 MarkDown 文件
                    Files.CopyDirectory(item.Origin, item.Destination);
                    //按照原文件夹的层级目录，在输出文件夹中创建相同的文件夹
                    Files.CopyDirectoryOnly(item.Origin, item.Destination);
                    //对 MarkDown 文件夹进行解析、编译以及样式处理
                    Run(item.Origin, item);
                    //处理所有文件后，为这些文件添加相对目录
                    BuildCatalog(item.Destination);
                }
                var t2 = DateTime.Now;
                Console.WriteLine($"Finish: {(int)(t2 - t1).TotalSeconds}s");
                Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                e.StackTrace.ToList().ForEach(p => Console.WriteLine(p));
                Console.ReadLine();
            }
        }

        /// <summary>
        /// 左侧的目录，HTML 格式
        /// </summary>
        static string Catalog;

        /// <summary>
        /// 目录中的绝对链接
        /// </summary>
        static MatchCollection CatalogLinks;

        /// <summary>
        /// 所有生成的 HTML 文件
        /// </summary>
        static readonly List<string> AllHtmlFiles = new List<string>();

        private static void Clear()
        {
            Catalog = string.Empty;
            AllHtmlFiles.Clear();
        }

        /// <summary>
        /// 以文件夹中的 MarkDown 文件进行解析、编译以及样式处理
        /// </summary>
        /// <param name="directorie">要处理的 MarkDown 文件夹</param>
        /// <param name="config">配置文件</param>
        static void Run(string directorie, ConfigItem config)
        {
            Catalog += "\r\n<nav class='nav nav-pills flex-column ml-2'>";
            foreach (var file in Directory.GetFiles(directorie))
            {
                if (Path.GetExtension(file) != ".md") continue;
                var relativeToOrigin = Path.GetRelativePath(config.Origin, file);
                //在配置文件中该文档是否根二级标题自动折叠
                var collapse = config.FolderJson != null && config.FolderJson["collapse"].ToList().Any(p => p.ToString().Equals(relativeToOrigin, StringComparison.OrdinalIgnoreCase));
                var (title, content, sideNav) = Convert(file, collapse);
                //生成后的文件路径
                var newFile = Path.Combine(config.Destination, relativeToOrigin.Replace(".md", ".html"));
                var git = Path.Combine(config.Git, relativeToOrigin);
                Build(newFile, content, title, sideNav, git, relativeToOrigin.Split("\\").Length - 1, config.Template, collapse);
                Catalog += $"<a class='ml-0 my-1 nav-link' href='{newFile.Replace("\\", "/")}' data-path='{relativeToOrigin.Replace("\\", "/").Replace(".md", "")}'>{title}</a>";
            }
            foreach (var dir in Directory.GetDirectories(directorie))
            {
                var dirName = dir.Split("\\").Reverse().ToList()[0];
                if (config.FolderJson != null)
                {
                    if(config.FolderJson["hidden"].Any(p => p.ToString() == dirName)) continue;
                    if(config.FolderJson["rename"] != null)
                        dirName = config.FolderJson["rename"][dirName]?.ToString() ?? dirName;
                }
                Catalog += $"<span class='ml-0 my-1 nav-link' data-icon='+'>{dirName}</span>";

                Run(dir, config);
            }
            Catalog += "\r\n</nav>";
        }

        /// <summary>
        /// 对单个 MarkDown 文件进行到 HTML 的转换
        /// </summary>
        /// <param name="file">文件名</param>
        /// <param name="collapse">所有二级标题下面的内容自动折叠，点击二级标题后展开或收缩。True 表示启用</param>
        /// <returns>输出元组（标题，内容，文章内的目录）</returns>
        static (string title, string content, string sideNav) Convert(string file, bool collapse)
        {
            MarkdownDocument document = new MarkdownDocument();
            document.Parse(File.ReadAllText(file).Replace("\\|", "&#124;"));
            //文章标题（首个标题的文本）
            string title = null;
            //文章内的目录
            var sideNav = string.Empty;
            //文章正文
            var content = string.Empty;
            bool startCollapse = false;

            var lastHeaderLevel = 0;
            foreach (var element in document.Blocks)
            {
                if (element.Type == MarkdownBlockType.Header && (element as HeaderBlock).HeaderLevel <= 3)
                {
                    var header = element as HeaderBlock;
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
                    sideNav += $"\r\n<a class='ml-{(header.HeaderLevel - 2) * 2}{(header.HeaderLevel == 1 ? " d-none" : "")} my-1 nav-link' href='{header.ToString().ToAnchorPoint()}'>{headerText}</a>";

                    lastHeaderLevel = header.HeaderLevel;
                }
                #region collapse
                //如果 collapse 为 true，则将 h2 下面的所有内容用 <div></div> 包裹起来
                if (collapse && (element as HeaderBlock)?.HeaderLevel == 2 && startCollapse)
                {
                    content += "</div>";
                    startCollapse = false;
                }
                #endregion

                content += element.ToHtml(collapse ? "collapse" : "");

                #region collapse
                if (collapse && (element as HeaderBlock)?.HeaderLevel == 2 && !startCollapse)
                {
                    content += "\r\n<div class='div-collapse p-2 px-4'>";
                    startCollapse = true;
                }
                #endregion
            }
            #region collapse
            if (collapse && startCollapse)
            {
                content += "</div>";
            }
            #endregion

            for (int i = 0; i < lastHeaderLevel - 0; i++)
            {
                sideNav += "\r\n</nav>";
            }
            return (title?.Trim(), content?.Trim(), sideNav?.Trim());
        }
        /// <summary>
        /// 根据 HTML 内容和网页模板，生成编译后的 HTML 文件
        /// </summary>
        /// <param name="path">生成后的文件路径</param>
        /// <param name="content">正文（HTML）</param>
        /// <param name="title">标题（TXT）</param>
        /// <param name="sideNav">文章内的目录（HTML）</param>
        /// <param name="depth">该文件相对于根目录（Origin）的层级深度</param>
        /// <param name="template">HTML 模板的文件名</param>
        /// <param name="collapse">是否对内容进行折叠</param>
        static void Build(string path, string content, string title, string sideNav, string git, int depth, string template, bool collapse)
        {
            var depthStr = string.Empty;
            for (int i = 0; i < depth; i++)
            {
                depthStr += "../";
            }
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine(File.ReadAllText(Path.Combine(template, "index.html"))
                    .Replace("{title}", title)
                    .Replace("{git}", git)
                    .Replace("{sideNav}", sideNav)
                    .Replace("{body}", content)
                    .Replace("{depth}", depthStr)
                    .Replace("_collapse", collapse.ToString().ToLower()));
                Console.WriteLine($"build: {path}");
            }
        }
        
        private static void BuildCatalog(string path)
        {
            void GetAllFiles(string _path)
            {
                Directory.GetFiles(_path).ToList().ForEach(p => AllHtmlFiles.Add(p));
                Directory.GetDirectories(_path).ToList().ForEach(p => GetAllFiles(p));
            };
            GetAllFiles(path);
            //查找目录中所有的绝对路径的链接
            CatalogLinks = new Regex("href='.*?\\.html'").Matches(Catalog);
            //将绝对路径的链接转为相对路径的链接
            Parallel.ForEach(AllHtmlFiles, (file) =>
            {
                if (Path.GetExtension(file) != ".html")
                    return;
                ProcessRelativePath(file, Catalog);
            });
        }

        /// <summary>
        /// 将目录中的绝对路径的链接转为相对路径的链接，交将目录插入到 HTML 文件中
        /// </summary>
        /// <param name="file">HTML 文件</param>
        /// <param name="catalog">目录（HTML）</param>
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
