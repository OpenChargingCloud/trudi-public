namespace TRuDI.HanAdapter.Example.ConfigModel
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using TRuDI.HanAdapter.Interface;

    public class HanAdapterExampleConfig
    {
        
        // Connect data

        public string DeviceId
        {
            get; set;
        }

        public string User
        {
            get; set;
        }

        public string Password
        {
            get; set;
        }

        public String IPAddress
        {
            get; set;
        }

        public int IPPort
        {
            get; set;
        }

        public TimeSpan TimeToConnect
        {
            get; set;
        }

        public string Cert
        {
            get; set;
        }

        public FirmwareVersion Version
        {
            get; set;
        }

        // AdapterContext 

        public List<ContractInfo> Contracts
        {
            get; set;
        }

        public BillingPeriod BillingPeriod
        {
            get; set;
        }

        public DateTime? Start
        {
            get; set;

        }

        public DateTime? End
        {
            get; set;
        }

        public bool WithLogData
        {
            get; set;
        }

        // Xml data

        public XmlConfig XmlConfig
        {
            get; set;
        }

        // If Taf7 is configured 
        public XDocument SupplierXml
        {
            get; set;
        }

        // The used Contract
        public ContractInfo Contract
        {
            get; set;
        }
    }
}
