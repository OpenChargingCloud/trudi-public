namespace TRuDI.Backend.Application
{
    using System.Collections.Generic;
    using System.Linq;

    using Serilog;

    using TRuDI.Backend.Exceptions;
    using TRuDI.HanAdapter.DrNeuhaus;
    using TRuDI.HanAdapter.Efr;
    using TRuDI.HanAdapter.Example;
    using TRuDI.HanAdapter.Kiwigrid;
    using TRuDI.HanAdapter.LandisGyr;
    using TRuDI.HanAdapter.Ppc;
    using TRuDI.HanAdapter.ThebenAG;
    using TRuDI.Models;

    public static class HanAdapterRepository
    {
        public static IReadOnlyList<HanAdapterInfo> AvailableAdapters => availableAdapters;

        private static readonly List<HanAdapterInfo> availableAdapters = new List<HanAdapterInfo>
            {
                new HanAdapterInfo("DNT", "Dr. Neuhaus Telekommunikation GmbH", typeof(HanAdapterDrNeuhaus)),
                new HanAdapterInfo("DVL", "devolo AG", typeof(HanAdapterKiwigrid)),
                new HanAdapterInfo("EFR", "EFR - Europäische Funk-Rundsteuerung GmbH", typeof(HanAdapterEfr)),
                new HanAdapterInfo("KIG", "Kiwigrid GmbH", typeof(HanAdapterKiwigrid)),
                new HanAdapterInfo("LGZ", "Landis+Gyr AG", typeof(HanAdapterLandisGyr)),
                new HanAdapterInfo("PPC", "Power Plus Communications AG", typeof(HanAdapterPpc)),
                new HanAdapterInfo("THE", "Theben AG", typeof(HanAdapterThebenAG)),
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
