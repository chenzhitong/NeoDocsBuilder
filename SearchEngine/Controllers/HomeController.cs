using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchEngine.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index(string k, string l = null)
        {
            if (string.IsNullOrEmpty(k)) return Redirect("/docs/index.html");
            var result = new List<Result>();

            foreach (var pages in Sources.Pages)
            {
                foreach (var line in pages.Lines)
                {
                    if (k.Split(' ').ToList().All(key => line.Contains(key, StringComparison.OrdinalIgnoreCase)))
                    {
                        var title = pages.Lines.FirstOrDefault(p => p.StartsWith("#"))?.TrimStart('#', ' ').Trim();
                        if (title == null) continue;
                        result.Add(new Result() { Line = line.Trim(), Link = pages.Link, Title = title});
                    }                    
                }
            }
            result = result.GroupBy(p => p.Link).OrderByDescending(p => p.Count()).Select(p => new Result() { 
                Link = p.Key,
                Line = p.FirstOrDefault().Line.Length > 50 ? p.FirstOrDefault().Line.Substring(0, 50) + "..." : p.FirstOrDefault().Line,
                Title = p.FirstOrDefault().Title }).ToList();

            if (!string.IsNullOrEmpty(l))
                result = result.Where(p => p.Link.Contains(l)).ToList();

            return Content(JsonConvert.SerializeObject(result), "application/json");
        }
    }
}
