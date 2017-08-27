namespace TRuDI.Backend.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    using TRuDI.Backend.Application;
    using TRuDI.Backend.Models;

    public class ProgressController : Controller
    {
        private readonly ApplicationState applicationState;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressController"/> class.
        /// </summary>
        /// <param name="applicationState">State of the application.</param>
        public ProgressController(ApplicationState applicationState)
        {
            this.applicationState = applicationState;
        }

        public IActionResult Index(ProgressDataViewModel model)
        {
            return this.View(model);
        }

        [HttpPost]
        public IActionResult CancelOperation()
        {
            this.applicationState.CancelOperation();
            return Ok();
        }

        [HttpGet]
        public IActionResult GetNextPageToLoad()
        {
            if (this.applicationState.NextPageAfterProgress != null)
            {
                return this.Ok(this.applicationState.NextPageAfterProgress);
            }

            return this.NotFound();
        }
    }
}
