namespace TRuDI.Backend.Controllers
{
    using System;
    using System.IO;

    using Microsoft.AspNetCore.Mvc;

    using TRuDI.Backend.Application;
    using TRuDI.Backend.Components;

    public class DataViewController : Controller
    {
        private readonly ApplicationState applicationState;

        public DataViewController(ApplicationState applicationState)
        {
            this.applicationState = applicationState;
        }

        public IActionResult Index()
        {
            return this.View();
        }

        public IActionResult ValidationError()
        {
            return this.View();
        }

        public FileResult DownloadXml()
        {
            var ms = new MemoryStream();
            this.applicationState.CurrentDataResult.Raw.Save(ms);
            ms.Position = 0;

            this.Response.Headers.Add("Content-Disposition", "attachment; filename=result.xml");
            return new FileStreamResult(ms, "text/xml");
        }

        public ViewComponentResult FilterLog(DateTime startTime, DateTime endTime, string filterText)
        {
            return this.ViewComponent(typeof(LogItemsView), new { startTime = startTime.Date, endTime = (endTime + TimeSpan.FromDays(1)).Date, filterText });
        }
    }
}
