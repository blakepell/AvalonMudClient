using Microsoft.AspNetCore.Mvc;

namespace Avalon.Web
{
    public class HomeController : Controller
    {
        [Route("/")]
        public IActionResult Index() => new OkResult();

    }
}
