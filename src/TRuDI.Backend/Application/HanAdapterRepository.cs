namespace TRuDI.Backend.Application
{
    using System.Collections.Generic;
    using System.Linq;

    using TRuDI.Backend.Exceptions;
    using TRuDI.HanAdapter.Example;
    using TRuDI.HanAdapter.Interface;

    public static class HanAdapterRepository
    {
        public static IReadOnlyList<HanAdapterInfo> AvailableAdapters => availableAdapters;

        private static List<HanAdapterInfo> availableAdapters = new List<HanAdapterInfo>
            {
            };


        public static void ActivateExampleHanAdapter()
        {
           availableAdapters.Add(new HanAdapterInfo("XXX", "Example GmbH", typeof(HanAdapterExample), "")); 
        }

        public static HanAdapterContainer LoadAdapter(string serverId)
        {
            var id = new ServerId(serverId);

            var adapterInfo = AvailableAdapters.FirstOrDefault(a => a.FlagId == id.FlagId);
            if(adapterInfo == null)
            {
                throw new UnknownManufacturerException { FlagId = id.FlagId };
            }

            return new HanAdapterContainer(adapterInfo, serverId);
        }
    }
}
