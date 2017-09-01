namespace TRuDI.Backend.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    using TRuDI.Backend.Application;

    public class BreadCrumbController : Controller
    {
        private readonly ApplicationState applicationState;

        public BreadCrumbController(ApplicationState applicationState)
        {
            this.applicationState = applicationState;
        }

        public IActionResult Index(int id)
        {
            return this.Redirect(this.applicationState.BreadCrumbTrail.BackTo(id));
        }
    }
}
