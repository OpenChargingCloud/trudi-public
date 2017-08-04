namespace TRuDI.HanAdapter.Example.Components
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    public class GatewayImageExampleView : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string deviceId)
        {
            return View(new GatewayImageExampleViewModel { DeviceId = deviceId });
        }
    }
}
