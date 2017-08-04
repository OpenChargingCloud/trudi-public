namespace TRuDI.HanAdapter.Example.Components
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    public class ManufacturerParametersExampleView : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
