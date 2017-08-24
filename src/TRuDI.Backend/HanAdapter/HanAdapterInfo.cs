namespace TRuDI.Backend.HanAdapter
{
    using System;
    using System.Reflection;
    using TRuDI.HanAdapter.Interface;

    public class HanAdapterInfo
    {
        private readonly Type adapterType;

        public HanAdapterInfo(string flagId, string manufacturerName, Type adapterType, string hash)
        {
            this.FlagId = flagId;
            this.ManufacturerName = manufacturerName;
            this.Hash = hash;

            this.adapterType = adapterType;
        }

        public string FlagId { get; }
        public string ManufacturerName { get; }

        public string Hash { get; }

        public string BaseNamespace => this.adapterType.Namespace;

        public Assembly Assembly => this.adapterType.Assembly;

        public IHanAdapter CreateInstance()
        {
            return Activator.CreateInstance(this.adapterType) as IHanAdapter;
        }
    }
}
