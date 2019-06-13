using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using System;
using System.IO;
using System.Linq;

namespace NeoDocsBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            //拷贝 CSS, JS, Lib 等文件夹
            Files.CopyDirectory(Config.Template, Config.Destination);
            Files.CopyDirectoryOnly(Config.Origin, Config.Destination);
            Run(Config.Origin, Config.Destination, Config.Template);
            Console.ReadLine();
        }

        static void Run(string origin, string destination, string template)
        {
            var files = Directory.GetFiles(origin);
            foreach (var file in files)
            {
                if (Path.GetExtension(file) != ".md")
                    continue;
                var filePathWithoutOrigin = string.Join("\\", file.Split("\\").ToArray().Skip(1)).Replace(".md", ".html");
                var (title, content) = Convert(Parse(file));
                Build(Path.Combine(destination, filePathWithoutOrigin), content, title, template);
            }
            var dirs = Directory.GetDirectories(origin);
            foreach (var dir in dirs)
            {
                Run(dir, destination, template);
            }
        }
        static MarkdownDocument Parse(string name)
        {
            MarkdownDocument document = new MarkdownDocument();
            document.Parse(File.ReadAllText(name));
            return document;
        }

        static (string title, string content) Convert(MarkdownDocument document)
        {
            var title = string.Empty;
            var content = string.Empty;
            foreach (var element in document.Blocks)
            {
                if (element.Type == MarkdownBlockType.Header)
                {
                    var header = element as HeaderBlock;
                    if (header.HeaderLevel == 1)
                        title = header.ToString();
                }
                content += element.ToHtml();
            }
            return (title.Trim(), content.Trim());
        }

        static void Build(string name, string content, string title, string template)
        {
            var path = Path.Combine(name);
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine(File.ReadAllText(Path.Combine(template, "index.html")).Replace("{title}", title).Replace("{body}", content));
                Console.WriteLine(name);
            }
        }

    }
}
