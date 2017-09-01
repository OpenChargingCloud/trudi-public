namespace TRuDI.Backend.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    using TRuDI.Backend.Application;

    public class OperatingModeSelectionController : Controller
    {
        private readonly ApplicationState applicationState;

        public OperatingModeSelectionController(ApplicationState applicationState)
        {
            this.applicationState = applicationState;
        }

        public IActionResult Index()
        {
            this.applicationState.BreadCrumbTrail.Reset();
            return this.View();
        }

        public IActionResult StartDisplayFunction()
        {
            this.applicationState.OperationMode = OperationMode.DisplayFunction;
            this.applicationState.CurrentSupplierFile = null;
            return this.RedirectToAction("Index", "Connect");
        }

        public IActionResult StartTransparencyFunction()
        {
            this.applicationState.OperationMode = OperationMode.TransparencyFunction;
            return this.RedirectToAction("Index", "SupplierFile");
        }

    }
}
