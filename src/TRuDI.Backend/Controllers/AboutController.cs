namespace TRuDI.Backend.Controllers
{
    using System.Reflection;

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
            var lastUrl = this.Request.Headers["Referer"].ToString();
            if (!this.applicationState.LastUrl.TryPeek(out var lastStoredUrl) || lastStoredUrl != lastUrl)
            {
                this.applicationState.LastUrl.Push(lastUrl);
            }

            return this.View();
        }
        public IActionResult Back()
        {
            return this.Redirect(this.applicationState.LastUrl.Pop());
        }
    }
}
