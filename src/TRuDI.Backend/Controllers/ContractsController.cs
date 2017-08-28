namespace TRuDI.Backend.Controllers
{
    using System;

    using Microsoft.AspNetCore.Mvc;

    using TRuDI.Backend.Application;
    using TRuDI.HanAdapter.Interface;

    public class ContractsController : Controller
    {
        private readonly ApplicationState applicationState;

        public ContractsController(ApplicationState applicationState)
        {
            this.applicationState = applicationState;
        }

        public IActionResult Index()
        {
            this.ViewData["ErrorMessage"] = this.applicationState.LastErrorMessage;
            return this.View();
        }


        public IActionResult StartReadout(
            int contractIndex,
            int billingPeriodIndex,
            DateTime startTime,
            DateTime endTime)
        {
            var contract = this.applicationState.Contracts[contractIndex];

            var ctx = new AdapterContext
                          {
                              Contract = contract,
                              BillingPeriod = contract.BillingPeriods[billingPeriodIndex],
                              Start = startTime,
                              End = endTime,
                          };

            this.applicationState.LoadData(ctx);
            ctx.WithLogdata = true;

            return this.Ok();
        }
    }
}
