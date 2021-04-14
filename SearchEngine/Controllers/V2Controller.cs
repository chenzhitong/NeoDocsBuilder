using Microsoft.AspNetCore.Mvc;

namespace SearchEngine.Controllers
{
    public class V2Controller : Controller
    {
        public IActionResult Index()
        {
            return Redirect("/v2/docs/index.html");
        }
    }
}