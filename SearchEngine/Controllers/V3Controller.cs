using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SearchEngine.Controllers
{
    public class V3Controller : Controller
    {
        public IActionResult Index()
        {
            return Redirect("/v3/docs/index.html");
        }
    }
}