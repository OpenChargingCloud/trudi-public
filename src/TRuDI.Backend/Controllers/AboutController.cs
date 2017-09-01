namespace TRuDI.Backend.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    using TRuDI.Backend.Application;

    public class AboutController : Controller
    {
        private readonly ApplicationState applicationState;

        public AboutController(ApplicationState applicationState)
        {
            this.applicationState = applicationState;
        }

        public IActionResult Index()
        {
            return this.View();
        }
    }
}
