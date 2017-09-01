namespace TRuDI.Backend.Components
{
    using System;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;

    using TRuDI.Backend.Application;

    public class LogItemsView : ViewComponent
    {
        private readonly ApplicationState applicationState;

        public LogItemsView(ApplicationState applicationState)
        {
            this.applicationState = applicationState;
        }

        public IViewComponentResult Invoke(DateTime startTime, DateTime endTime, string filterText)
        {
            if (endTime == DateTime.MinValue)
            {
                endTime = DateTime.MaxValue;
            }

            return this.View(
                this.applicationState.CurrentDataResult.Model.LogEntries.Where(
                    e => e.LogEvent != null
                         && e.LogEvent.Timestamp >= startTime && e.LogEvent.Timestamp <= endTime
                         && (string.IsNullOrWhiteSpace(filterText) || e.LogEvent.Text.Contains(filterText))));
        }
    }
}
