namespace TRuDI.Backend.Controllers
{
    using System;
    using System.IO;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;

    using TRuDI.Backend.Application;
    using TRuDI.Backend.Components;
    using TRuDI.Backend.Utils;

    public class DataViewController : Controller
    {
        private readonly ApplicationState applicationState;

        public DataViewController(ApplicationState applicationState)
        {
            this.applicationState = applicationState;
        }

        public IActionResult Index()
        {
            this.applicationState.BreadCrumbTrail.Add("Abrechnungsdaten", "/DataView", false);
            this.applicationState.SideBarMenu.Clear();
            this.applicationState.SideBarMenu.Add(null, null);
            this.applicationState.SideBarMenu.Add("Zertifikate", "/CertificateDetails");
            this.applicationState.SideBarMenu.Add("Daten exportieren", "/DataView/DownloadXml");

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

        public ViewComponentResult FilterLog(DateTime startTime, DateTime endTime, string filterText, string filterLevel)
        {
            return this.ViewComponent(typeof(LogItemsView), new { startTime = startTime, endTime = endTime, filterText, filterLevel });
        }

        public ViewComponentResult FilterOvl(string ovlId, DateTime startTime)
        {
            var ovl = this.applicationState.CurrentDataResult.OriginalValueLists.FirstOrDefault(
                l => l.GetOriginalValueListIdent() == ovlId);

            return this.ViewComponent(typeof(OriginalValueListView), new { ovl, startTime});
        }

        public ViewComponentResult SelectTariffViewDay(DateTime timestamp)
        {
            return this.ViewComponent(typeof(TariffDataView), new { timestamp = timestamp.Date });
        }
    }
}
