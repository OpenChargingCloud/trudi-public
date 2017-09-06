namespace TRuDI.Backend.MessageHandlers
{
    using System;
    using System.Diagnostics;
    using System.Net.WebSockets;
    using System.Threading.Tasks;
    using WebSocketManager;

    public class NotificationsMessageHandler : WebSocketHandler
    {
        private WebSocket lastSocket;

        public NotificationsMessageHandler(WebSocketConnectionManager webSocketConnectionManager) : base(webSocketConnectionManager)
        {
        }

        public override Task OnConnected(WebSocket socket)
        {
            this.lastSocket = socket;

            Trace.WriteLine($"OnConnected: \"{socket}\"");
            return base.OnConnected(socket);
        }

        public override Task OnDisconnected(WebSocket socket)
        {
            Trace.WriteLine($"OnDisconnected: \"{socket}\"");
            return base.OnDisconnected(socket);
        }

        public async Task ProgressUpdate(string message, int progress)
        {
            Trace.WriteLine($"ProgressUpdate: \"{message}\", {progress}");

            if (this.lastSocket == null || this.lastSocket.State == WebSocketState.Closed)
            {
                return;
            }

            try
            {
                await this.InvokeClientMethodAsync(this.WebSocketConnectionManager.GetId(this.lastSocket), "ProgressUpdate", new object[] { System.Net.WebUtility.HtmlEncode(message), progress });
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"LoadNextPage: {ex.Message}");
            }
        }

        public async Task LoadNextPage(string url)
        {
            Trace.WriteLine($"LoadNextPage: \"{url}\"");

            if(this.lastSocket == null)
            {
                return;
            }

            try
            {
                await this.InvokeClientMethodAsync(this.WebSocketConnectionManager.GetId(this.lastSocket), "LoadNextPage", new object[] { url });
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"LoadNextPage: {ex.Message}");
            }
        }
    }
}
