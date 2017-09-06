namespace TRuDI.Backend.Application
{
    using System;
    using System.Reflection;

    using TRuDI.Backend.Utils;
    using TRuDI.HanAdapter.Interface;
    using TRuDI.TafAdapter.Interface;

    public class TafAdapterInfo
    {
        private readonly Type adapterType;

        public TafAdapterInfo(TafId tafId, string description, Type adapterType)
        {
            this.TafId = tafId;
            this.Description = description;
            this.Hash = DigestUtils.GetDigestFromAssembly(adapterType);

            this.adapterType = adapterType;
        }

        public TafId TafId { get; }

        public string Description { get; }

        public string Hash { get; }

        public string BaseNamespace => this.adapterType.Namespace;

        public Assembly Assembly => this.adapterType.Assembly;

        public ITafAdapter CreateInstance()
        {
            return Activator.CreateInstance(this.adapterType) as ITafAdapter;
        }
    }
}
