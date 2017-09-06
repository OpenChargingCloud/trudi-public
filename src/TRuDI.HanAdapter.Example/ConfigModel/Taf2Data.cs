using System;
using System.Collections.Generic;
using System.Text;
using TRuDI.HanAdapter.XmlValidation.Models;

namespace TRuDI.HanAdapter.Example.ConfigModel
{
    class Taf2Data
    {
        public Taf2Data(ObisId obisID)
        {
            this.ObisID = obisID;
            this.Data = new List<(DateTime timestamp, int tariff, int value)>();
        }

        public ObisId ObisID { get; set; }

        public List<(DateTime timestamp, int tariff, int value)> Data
        {
            get; set;
        }
    }
}
