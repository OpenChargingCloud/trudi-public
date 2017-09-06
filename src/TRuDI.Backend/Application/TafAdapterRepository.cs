namespace TRuDI.Backend.Application
{
    using System.Collections.Generic;
    using System.Linq;

    using TRuDI.Backend.Exceptions;
    using TRuDI.HanAdapter.Interface;
    using TRuDI.TafAdapter.Interface;
    using TRuDI.TafAdapter.Taf1;
    using TRuDI.TafAdapter.Taf2;

    public static class TafAdapterRepository
    {
        public static IReadOnlyList<TafAdapterInfo> AvailableAdapters => availableAdapters;

        private static readonly List<TafAdapterInfo> availableAdapters = new List<TafAdapterInfo>
            {
                new TafAdapterInfo(TafId.Taf1, "Standard Adapter für TAF-1", typeof(TafAdapterTaf1)),
                new TafAdapterInfo(TafId.Taf2, "Standard Adapter für TAF-2", typeof(TafAdapterTaf2)),
            };

        public static ITafAdapter LoadAdapter(TafId tafId)
        {
            var adapterInfo = AvailableAdapters.FirstOrDefault(a => a.TafId == tafId);
            if(adapterInfo == null)
            {
                throw new UnknownTafAdapterException { TafId = tafId };
            }

            return adapterInfo.CreateInstance();
        }
    }
}
