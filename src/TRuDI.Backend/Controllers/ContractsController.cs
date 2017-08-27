namespace TRuDI.Backend.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    using TRuDI.Backend.Application;

    public class ContractsController : Controller
    {
        private readonly ApplicationState applicationState;

        public ContractsController(ApplicationState applicationState)
        {
            this.applicationState = applicationState;
        }

        public IActionResult Index()
        {
            return this.View();
        }
    }
}
