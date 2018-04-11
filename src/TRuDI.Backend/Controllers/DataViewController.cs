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
#if DEBUG
            this.applicationState.SideBarMenu.Add("Daten exportieren", "/DataView/DownloadXml");
#else
            this.applicationState.SideBarMenu.Add("Daten exportieren", "showSaveFileDialog('/DataView/DownloadXml')", useOnClick:true);
#endif
            return this.View();
        }

        public FileResult DownloadXml()
        {
            var ms = new MemoryStream();
            this.applicationState.CurrentDataResult.VersionedExportXml.Save(ms);
            ms.Position = 0;

            this.Response.Headers.Add("Content-Disposition", "attachment; filename=result.xml");
            return new FileStreamResult(ms, "text/xml");
        }

        [HttpPost]
        public IActionResult DownloadXml(string filename)
        {
            try
            {
                var ms = new MemoryStream();
                this.applicationState.CurrentDataResult.VersionedExportXml.Save(ms);
                ms.Position = 0;

                System.IO.File.WriteAllBytes(filename, ms.ToArray());
            }
            catch (Exception)
            {
            }

            return this.Ok();
        }

        public ViewComponentResult FilterLog(DateTime startTime, DateTime endTime, string filterText, string filterLevel)
        {
            return this.ViewComponent(typeof(LogItemsView), new { startTime = startTime, endTime = endTime, filterText, filterLevel });
        }

        public ViewComponentResult FilterOvl(string ovlId, DateTime startTime)
        {
            var ovl = this.applicationState.CurrentDataResult.OriginalValueLists.FirstOrDefault(
                l => l.GetOriginalValueListIdent() == ovlId);

            return this.ViewComponent(typeof(OriginalValueListView), new { ovl, startTime });
        }

        public ViewComponentResult ShowErrorsList(string ovlId)
        {
            var ovl = this.applicationState.CurrentDataResult.OriginalValueLists.FirstOrDefault(
                l => l.GetOriginalValueListIdent() == ovlId);

            return this.ViewComponent(typeof(OriginalValueListErrorsView), new { ovl });
        }
    }
}
