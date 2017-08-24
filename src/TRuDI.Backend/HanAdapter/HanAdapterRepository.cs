namespace TRuDI.Backend.HanAdapter
{
    using System;
    using System.Linq;

    using TRuDI.HanAdapter.Example;
    using TRuDI.HanAdapter.Interface;

    public static class HanAdapterRepository
    {
        public static HanAdapterInfo[] AvailableAdapters = new[]
            {
                new HanAdapterInfo("XXX", "Example GmbH", typeof(HanAdapterExample), ""),
            };

        public static HanAdapterContainer LoadAdapter(string serverId)
        {
            var id = new ServerId(serverId);

            var adapterInfo = AvailableAdapters.FirstOrDefault(a => a.FlagId == id.FlagId);
            if(adapterInfo == null)
            {
                throw new Exception($"Unknown manufacturer: {id.FlagId}");
            }

            return new HanAdapterContainer(adapterInfo, serverId);
        }
    }
}
