using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SearchEngine.Models;

namespace SearchEngine.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index(string k, string l)
        {
            if (string.IsNullOrEmpty(k)) return Redirect("/docs/index.html");
            var result = new List<Result>();

            foreach (var pages in Sources.Pages)
            {
                foreach (var line in pages.Lines)
                {
                    if (k.Split(' ').ToList().All(key => line.Contains(key, StringComparison.OrdinalIgnoreCase)))
                    {
                        result.Add(new Result() { Line = line, Link = pages.Link, Title = pages.Lines.Length > 0 ? pages.Lines[0].TrimStart('#', ' ') : string.Empty });
                    }                    
                }
            }
            
            return Content(JsonConvert.SerializeObject(result.Where(p => p.Link.Contains(l))), "application/json");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
