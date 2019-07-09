using System;
using System.IO;

namespace NeoDocsBuilder
{
    public static class YmlConverter
    {
        public static string ToHtml(this string[] yml, string pathBase)
        {
            var nav = "\r\n<nav class='nav nav-pills flex-column ml-3'>";
            var navend = "\r\n</nav>";
            var catalog = nav;
            Link a = new Link();
            var lastDepth = 0;
            foreach (var line in yml)
            {
                var splitLine = line.Trim().Split(":");
                switch (splitLine[0])
                {
                    case "- name":
                        if (!string.IsNullOrEmpty(a.Name))
                        {
                            catalog += a.ToHtml();
                            a = new Link();
                        }
                        var depth = GetDepth(line);
                        for (int i = 0; i < depth - lastDepth; i++)
                        {
                            catalog += nav;
                        }
                        for (int i = 0; i < lastDepth - depth; i++)
                        {
                            catalog += navend;
                        }
                        lastDepth = depth;
                        a.Name = splitLine[1].Trim();
                        break;
                    case "href":
                        a.Href = splitLine[1].Trim();
                        LinkCheck(pathBase, a.Href);
                        break;
                }
                
            }
            if (a.Complete())
            {
                catalog += a.ToHtml();
            }
            for (int i = 0; i < lastDepth - 0 + 1; i++)
            {
                catalog += navend;
            }
            return catalog;
        }

        private static int GetDepth(string line)
        {
            int sum = 0;
            foreach (var item in line)
            {
                if (item == ' ')
                    sum++;
                else
                    break;
            }
            return sum / 2;
        }
        public static int linkCount = 0;
        public static int errorLinkCount = 0;
        private static void LinkCheck(string pathBase, string link)
        {
            var fullLink = $"{pathBase}{link.TrimStart('/').Replace("/", "\\")}";
            if (Path.GetExtension(fullLink) != ".md") return;
            linkCount++;
            if (File.Exists(fullLink))
            {
                return;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"File: {pathBase}\r\nLink: {link}");
                Console.ForegroundColor = ConsoleColor.White;
                errorLinkCount++;
                return;
            }
        }
    }
}
