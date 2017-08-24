namespace TRuDI.Backend.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using TRuDI.Backend.HanAdapter;

    public class GatewayDetailsController : Controller
    {
        private readonly ApplicationState applicationState;

        private string lastUrl;

        public GatewayDetailsController(ApplicationState applicationState)
        {
            this.applicationState = applicationState;
        }

        public IActionResult Index()
        {
            this.applicationState.LastUrl = Request.Headers["Referer"].ToString();
            ViewData["IsGatewayDetails"] = true;
            return this.View();
        }

        public IActionResult Back()
        {
            return Redirect(this.applicationState.LastUrl);
        }
    }
}
