namespace TRuDI.Backend.Components
{
    using System;

    using Microsoft.AspNetCore.Mvc;

    using TRuDI.Backend.Application;

    public class TariffDataView : ViewComponent
    {
        private readonly ApplicationState applicationState;

        public TariffDataView(ApplicationState applicationState)
        {
            this.applicationState = applicationState;
        }

        public IViewComponentResult Invoke(DateTime timestamp)
        {
            return this.View(timestamp);
        }
    }
}
