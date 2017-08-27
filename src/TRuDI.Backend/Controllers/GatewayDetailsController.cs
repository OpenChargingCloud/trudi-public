﻿namespace TRuDI.Backend.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    using TRuDI.Backend.Application;

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
            this.applicationState.LastUrl.Push(this.Request.Headers["Referer"].ToString());
            this.ViewData["IsGatewayDetails"] = true;
            return this.View();
        }

        public IActionResult Back()
        {
            return this.Redirect(this.applicationState.LastUrl.Pop());
        }
    }
}
