namespace TRuDI.Backend.Components
{
    using System;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;

    using TRuDI.Backend.Application;
    using TRuDI.Models;

    public class LogItemsView : ViewComponent
    {
        private readonly ApplicationState applicationState;

        public LogItemsView(ApplicationState applicationState)
        {
            this.applicationState = applicationState;
        }

        private int MinLogLevel(string filterLogLevel)
        {
            if (String.IsNullOrEmpty(filterLogLevel))
            {
                return 1;
            }

            if (filterLogLevel.Contains("all"))
            {
                return 1;
            }
            if (filterLogLevel.Contains("warn"))
            {
                return 2;
            }

            if (filterLogLevel.Contains("error"))
            {
                return 3;
            }

            return 1;
        }

        public IViewComponentResult Invoke(DateTime startTime, DateTime endTime, string filterText, string filterLevel)
        {
            startTime = startTime.DayStart();

            if (endTime == DateTime.MinValue)
            {
                endTime = DateTime.MaxValue;
            }
            else
            {
                endTime = endTime.DayEnd();
            }

            if (!string.IsNullOrWhiteSpace(filterText))
            {
                filterText = filterText.ToLowerInvariant();
            }

            return this.View(
                this.applicationState.CurrentDataResult.Model.LogEntries.Where(
                    e => e.LogEvent != null
                         && e.LogEvent.Timestamp >= startTime && e.LogEvent.Timestamp <= endTime
                         && (string.IsNullOrWhiteSpace(filterText) || e.LogEvent.Text.ToLowerInvariant().Contains(filterText))
                         && (string.IsNullOrWhiteSpace(filterLevel) || (int)e.LogEvent.Level >= this.MinLogLevel(filterLevel))));
        }
    }
}
