namespace TRuDI.Backend.Controllers
{
    using System.IO;

    using Microsoft.AspNetCore.Mvc;

    using TRuDI.Backend.Application;

    public class DataViewController : Controller
    {
        private readonly ApplicationState applicationState;

        public DataViewController(ApplicationState applicationState)
        {
            this.applicationState = applicationState;
        }

        public IActionResult Index()
        {
            this.ViewData["ErrorMessage"] = this.applicationState.LastErrorMessage;
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
    }
}
