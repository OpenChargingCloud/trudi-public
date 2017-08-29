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

        public IViewComponentResult Invoke(OriginalValueList ovl, DateTime startTime, DateTime endTime)
        {
            if (endTime == DateTime.MinValue)
            {
                endTime = DateTime.MaxValue;
            }

            var items = ovl.GetReadings(startTime, endTime);

            return this.View(new OriginalValueListRange(ovl, items));
        }
    }
}
