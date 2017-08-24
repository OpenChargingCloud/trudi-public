namespace TRuDI.Backend.Models
{
    public class ConnectDataViewModel
    {
        public string DeviceId { get; set; }

        public AuthMode AuthMode { get; set; } = AuthMode.UserPassword;

        public string Username { get; set; }

        public string Password { get; set; }

        public string Address { get; set; }

        public ushort Port { get; set; } = 443;
    }
}
