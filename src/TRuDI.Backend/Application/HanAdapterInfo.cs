namespace TRuDI.Backend.Application
{
    using System;
    using System.Reflection;

    using TRuDI.Backend.Utils;
    using TRuDI.HanAdapter.Example;
    using TRuDI.HanAdapter.Interface;

    public class HanAdapterInfo
    {
        private readonly Type adapterType;

        public HanAdapterInfo(string flagId, string manufacturerName, Type adapterType)
        {
            this.FlagId = flagId;
            this.ManufacturerName = manufacturerName;
            this.Hash = DigestUtils.GetDigestFromAssembly(adapterType);
            

            this.adapterType = adapterType;
        }

        public string FlagId { get; }
        public string ManufacturerName { get; }

        public string Hash { get; }

        public string BaseNamespace => this.adapterType.Namespace;

        public Assembly Assembly => this.adapterType.Assembly;

        public IHanAdapter CreateInstance()
        {
            if (this.adapterType.Name == nameof(HanAdapterExample))
            {
                return new HanAdapterExample(Program.CommandLineArguments.TestConfiguration);
            }

            return Activator.CreateInstance(this.adapterType) as IHanAdapter;
        }
    }
}
