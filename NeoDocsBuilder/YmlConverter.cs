using System.Collections.Generic;
using System.IO;

namespace NeoDocsBuilder
{
    public static class YmlConverter
    {
        public static string ToHtml(string file, string pathBase)
        {
            var yml = File.ReadAllLines(file);
            var nav = "\r\n<nav class='nav nav-pills flex-column ml-3'>";
            var navend = "\r\n</nav>";
            var paragraph = "\r\n<p></p>";
            var catalog = nav;
            Link a = new();
            var lastDepth = 0;
            foreach (var line in yml)
            {
                var splitLine = line.Trim().Split(":");
                switch (splitLine[0])
                {
                    case "":
                        if (!string.IsNullOrEmpty(a.Name))
                        {
                            catalog += a.ToHtml();
                            a = new Link();
                        }
                        for (int i = 0; i < lastDepth; i++)
                        {
                            catalog += navend;
                        }
                        catalog += paragraph;
                        lastDepth = 0;
                        break;
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
                        if(splitLine[1].Trim().StartsWith("http") && !string.IsNullOrEmpty(splitLine[2]))
                            a.Href =$"{splitLine[1].Trim()}:{splitLine[2].Trim()}";
                        else
                            a.Href = splitLine[1].Trim();
                        LinkCheck(file, pathBase, a.Href);
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

        public static readonly List<string> ErrorLogs = new();
        private static void LinkCheck(string file, string pathBase, string link)
        {
            var fullLink = $"{pathBase}{link.TrimStart('/').Replace("v2/", "").Replace("/", "\\")}";
            if (Path.GetExtension(fullLink) != ".md") return;
            if (File.Exists(fullLink))
            {
                return;
            }
            else
            {
                ErrorLogs.Add($"Error link: {file} ({link})");
                return;
            }
        }
    }
}
