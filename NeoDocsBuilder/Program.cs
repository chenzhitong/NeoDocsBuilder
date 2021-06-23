using Markdig;
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
            Config.ConfigFile = "config.json"; //设置配置文件

            foreach (var item in Config.ConfigList)
            {
                AllMdFiles.Clear();
                GetAllMdFiles(item.Origin);

                //复制模板到网站根目录，包括 CSS、JS、字体、图片等，不包含 .md .json .yml
                Console.WriteLine("Copy template files……");
                Files.CopyDirectory("template", "wwwroot");

                //复制源文件夹中的资源文件到输出目录，包括图片等，不包含 .md .json .yml
                Console.WriteLine("Copy source files……");
                Files.CopyDirectory(item.Origin, item.Destination);

                Console.WriteLine("Build catalog……");
                var catalog = YmlConverter.ToHtml(Path.Combine(item.Origin, "toc.yml"), Path.GetFullPath(Config.ConfigFile).Replace(Config.ConfigFile, ""));
                AllMdFiles.ForEach(md =>
                {
                    if (!catalog.Contains(md.Replace("\\", "/").Replace(".md", ".html")))
                        YmlConverter.ErrorLogs.Add($"The file is not in the catalog: {md}");
                });
                BuildMarkDown(catalog, item); //对 MarkDown 文件夹进行解析、编译以及样式处理
            }
            var t2 = DateTime.Now;
            Console.WriteLine($"Finish: {(int)(t2 - t1).TotalSeconds}s");


            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!string.IsNullOrEmpty(YmlConverter.ErrorLogs.ToString()))
            {
                Console.WriteLine(string.Join("\r\n", YmlConverter.ErrorLogs.ToArray()));
                Console.WriteLine($"Catalog Error Link: {YmlConverter.ErrorLogs.Count}");
            }
            Console.ForegroundColor = ConsoleColor.White;

            try { File.WriteAllText("log.txt", $"{DateTime.Now}"); } catch (Exception) { }

            Console.WriteLine("Press 'Enter' key in 3 seconds to pause...");
            Thread t = new(new ThreadStart(ConsolePause));
            t.Start();
            var t3 = DateTime.Now;
            while (true)
            {
                if ((DateTime.Now - t3).TotalSeconds > 3 && !isPause)
                    Environment.Exit(0);
                Thread.Sleep(100);
            }
        }

        static bool isPause;

        private static void ConsolePause()
        {
            if (Console.ReadKey().Key == ConsoleKey.Enter)
                isPause = true;
        }

        static readonly List<string> AllMdFiles = new();

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
            Parallel.ForEach(AllMdFiles, file =>
            {
                var relativeToOrigin = Path.GetRelativePath(config.Origin, file);
                //在配置文件中该文档是否根二级标题自动折叠
                var collapse = config.FolderJson != null && config.FolderJson["collapse"].ToList().Any(p => p.ToString().Equals(relativeToOrigin, StringComparison.OrdinalIgnoreCase));
                //生成后的文件路径
                var newFile = Path.Combine(config.Destination, relativeToOrigin.Replace(".md", ".html")).ToLower();
                var git = Path.Combine(config.Git, relativeToOrigin);
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
                Build(newFile, catalog, Markdown.ToHtml(File.ReadAllText(file), pipeline), "Test", "", git, collapse);
            });
        }

        /// <summary>
        /// 根据 HTML 内容和网页模板，生成编译后的 HTML 文件
        /// </summary>
        /// <param name="path">生成后的文件路径</param>
        /// <param name="catalog">目录（HTML）</param>
        /// <param name="content">正文（HTML）</param>
        /// <param name="title">标题（TXT）</param>
        /// <param name="sideNav">文章内的目录（HTML）</param>
        /// <param name="collapse">是否对内容进行折叠</param>
        static void Build(string path, string catalog, string content, string title, string sideNav, string git, bool collapse)
        {
            try
            {
                using StreamWriter sw = new(path);
                sw.WriteLine(File.ReadAllText("template/template.html")
                .Replace("{title}", title)
                .Replace("{git}", git)
                .Replace("{sideNav}", sideNav)
                .Replace("{body}", content)
                .Replace("{catalog}", catalog)
                .Replace("_collapse", collapse.ToString().ToLower()));
                Console.WriteLine($"build: {path}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
