namespace TRuDI.Backend.Application
{
    using System.Collections.Generic;
    using System.Linq;

    using Serilog;

    using TRuDI.Backend.Exceptions;
    using TRuDI.HanAdapter.Example;
    using TRuDI.Models;

    public static class HanAdapterRepository
    {
        public static IReadOnlyList<HanAdapterInfo> AvailableAdapters => availableAdapters;

        private static readonly List<HanAdapterInfo> availableAdapters = new List<HanAdapterInfo>
            {
            };


        public static void ActivateExampleHanAdapter()
        {
           availableAdapters.Add(new HanAdapterInfo("XXX", "Example GmbH", typeof(HanAdapterExample))); 
        }

        public static HanAdapterContainer LoadAdapter(string serverId)
        {
            Log.Information("Loading HAN adapter for: {0}", serverId);

            var id = new ServerId(serverId);

            var adapterInfo = AvailableAdapters.FirstOrDefault(a => a.FlagId == id.FlagId);
            if(adapterInfo == null)
            {
                Log.Error("Unknown HAN adapter: {0}", id.FlagId);
                throw new UnknownManufacturerException { FlagId = id.FlagId };
            }

            return new HanAdapterContainer(adapterInfo, serverId);
        }
    }
}
