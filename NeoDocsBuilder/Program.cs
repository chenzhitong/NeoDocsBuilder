using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace NeoDocsBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            var t1 = DateTime.Now;
            Config.ConfigFile = args.Length > 1 ? args[0] : "config.json"; //设置配置文件

            foreach (var item in Config.ConfigList)
            {
                Console.WriteLine("Beginning……");
                AllMdFiles.Clear();
                GetAllMdFiles(item.Origin);

                Console.WriteLine("Copy template files……");
                Files.CopyDirectory(item.Template, item.WebRoot); //复制模板到网站根目录，包括 CSS、JS、字体、图片等，不包含 .md .json .yml

                Console.WriteLine("Copy source files……");
                Files.CopyDirectory(item.Origin, item.Destination); //复制源文件夹中的资源文件到输出目录，包括图片等，不包含 .md .json .yml

                Console.WriteLine("Build catalog……");
                var catalog = YmlConverter.ToHtml(item.Catalog, Path.GetFullPath(Config.ConfigFile).Replace(Config.ConfigFile, ""));
                BuildMarkDown(catalog, item); //对 MarkDown 文件夹进行解析、编译以及样式处理
            }
            var t2 = DateTime.Now;
            Console.WriteLine($"Finish: {(int)(t2 - t1).TotalSeconds}s");


            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!string.IsNullOrEmpty(MdConverter.errorLog.ToString()))
            {
                Console.WriteLine(MdConverter.errorLog.ToString());
                Console.WriteLine($"Content Error Link: {MdConverter.errorLinkCount}/{MdConverter.linkCount}");
            }

            if (!string.IsNullOrEmpty(YmlConverter.errorLog.ToString()))
            {
                Console.WriteLine(YmlConverter.errorLog.ToString());
                Console.WriteLine($"Catalog Error Link: {YmlConverter.errorLinkCount}/{YmlConverter.linkCount}");
            }
            Console.ForegroundColor = ConsoleColor.White;

            try
            {
                File.WriteAllText("log.txt", $"{DateTime.Now}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            //Console.WriteLine("Press any key to continue...");
            //Console.ReadKey();
        }

        static readonly List<string> AllMdFiles = new List<string>();

        /// <summary>
        /// 获得当前目录下所有文件列表
        /// </summary>
        /// <param name="path"></param>
        private static void GetAllMdFiles(string path)
        {
            Directory.GetFiles(path).ToList().ForEach(p => { if (Path.GetExtension(p) == ".md") AllMdFiles.Add(p); });
            Directory.GetDirectories(path).ToList().ForEach(p => GetAllMdFiles(p));
        }

        /// <summary>
        /// 以文件夹中的 MarkDown 文件进行解析、编译以及样式处理
        /// </summary>
        /// <param name="config">配置文件</param>
        static void BuildMarkDown(string catalog, ConfigItem config)
        {
            Parallel.ForEach(AllMdFiles, file => {
                var relativeToOrigin = Path.GetRelativePath(config.Origin, file);
                //在配置文件中该文档是否根二级标题自动折叠
                var collapse = config.FolderJson != null && config.FolderJson["collapse"].ToList().Any(p => p.ToString().Equals(relativeToOrigin, StringComparison.OrdinalIgnoreCase));
                var (title, content, sideNav) = Convert(file, collapse);
                //生成后的文件路径
                var newFile = Path.Combine(config.Destination, relativeToOrigin.Replace(".md", ".html"));
                var git = Path.Combine(config.Git, relativeToOrigin);
                Build(newFile, catalog, content, title, sideNav, git, config.Template, collapse);
            });
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
            for (int index = 0; index < document.Blocks.Count; index++)
            {
                var element = document.Blocks[index];
                var html = element.ToHtml(file, collapse ? $"collapse{index.ToString()}" : index.ToString());
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
                    xml.LoadXml(html);
                    var headerText = xml.InnerText;
                    title = title ?? headerText;
                    sideNav += $"\r\n<a class='ml-{(header.HeaderLevel - 2) * 2}{(header.HeaderLevel == 1 ? " d-none" : "")} my-1 nav-link' href='{header.ToString().ToAnchorPoint(index.ToString())}'>{headerText}</a>";

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

                content += html;

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
        /// <param name="catalog">目录（HTML）</param>
        /// <param name="content">正文（HTML）</param>
        /// <param name="title">标题（TXT）</param>
        /// <param name="sideNav">文章内的目录（HTML）</param>
        /// <param name="depth">该文件相对于根目录（Origin）的层级深度</param>
        /// <param name="template">HTML 模板的文件名</param>
        /// <param name="collapse">是否对内容进行折叠</param>
        static void Build(string path, string catalog, string content, string title, string sideNav, string git, string template, bool collapse)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine(File.ReadAllText(Path.Combine(template, "template.html"))
                        .Replace("{title}", title)
                        .Replace("{git}", git)
                        .Replace("{sideNav}", sideNav)
                        .Replace("{body}", content)
                        .Replace("{catalog}", catalog)
                        .Replace("_collapse", collapse.ToString().ToLower()));
                    Console.WriteLine($"build: {path}");
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
