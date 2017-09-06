namespace TRuDI.Backend.Components
{
    using System;

    using Microsoft.AspNetCore.Mvc;

    using TRuDI.Backend.Application;
    using TRuDI.HanAdapter.XmlValidation.Models;

    public class OriginalValueListView : ViewComponent
    {
        private readonly ApplicationState applicationState;

        public OriginalValueListView(ApplicationState applicationState)
        {
            this.applicationState = applicationState;
        }

        public IViewComponentResult Invoke(OriginalValueList ovl, DateTime startTime)
        {
            startTime = startTime.Date;
            var endTime = startTime + TimeSpan.FromDays(1);

            var items = ovl.GetReadings(startTime, endTime);

            return this.View(new OriginalValueListRange(ovl, items));
        }
    }
}
