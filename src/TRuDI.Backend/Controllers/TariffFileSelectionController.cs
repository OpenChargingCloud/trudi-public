namespace TRuDI.Backend.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    using TRuDI.Backend.Application;

    public class TariffFileSelectionController : Controller
    {
        private readonly ApplicationState applicationState;

        public TariffFileSelectionController(ApplicationState applicationState)
        {
            this.applicationState = applicationState;
        }

        public IActionResult Index()
        {
            return this.View();
        }
    }
}
