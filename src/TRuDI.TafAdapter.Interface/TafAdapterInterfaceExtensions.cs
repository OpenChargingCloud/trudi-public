namespace TRuDI.TafAdapter.Interface
{
    using System.Collections.Generic;
    using System.Linq;
    using TRuDI.HanAdapter.XmlValidation.Models;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;
    using TRuDI.HanAdapter.XmlValidation.Models.CheckData;

    public static class TafAdapterInterfaceExtensions
    {
        public static List<Register> GetRegister(this UsagePointLieferant supplier)
        {
            var register = new List<Register>();
            var tariffStages = supplier.AnalysisProfile.TariffStages;

            foreach (TariffStage stage in tariffStages)
            {
                var reg = new Register()
                {
                    ObisCode = new ObisId(stage.ObisCode),
                    TariffId = stage.TariffNumber,
                    Amount = 0
                };
                register.Add(reg);
            }

            var errorRegister = register.Where(r => r.ObisCode.E == 63).ToList() ?? new List<Register>();

            if( errorRegister.Count > 0)
            {
                foreach(Register reg in errorRegister)
                {
                    switch (reg.ObisCode.C)
                    {
                        case 1:
                            reg.TariffId = 163;
                            break;
                        case 2:
                            reg.TariffId = 263;
                            break;
                        default:
                            reg.TariffId = 63;
                            break;
                    }
                }
            }
            else
            {
                var first = true;
                var seccond = true;
                foreach (Register reg in register.ToList())
                {
                    if(first && reg.ObisCode.C == 1)
                    {
                        var oc = reg.ObisCode.ToHexString();
                        var errRegister = new Register()
                        {
                            ObisCode = new ObisId(oc.Substring(0, 4) + "63" + oc.Substring(6)),
                            TariffId = 163,
                            Amount = 0
                        };

                        register.Add(errRegister);
                        first = false;
                    }
                    else if(seccond && reg.ObisCode.C == 2)
                    {
                        var oc = reg.ObisCode.ToHexString();
                        var errRegister = new Register()
                        {
                            ObisCode = new ObisId(oc.Substring(0, 4) + "63" + oc.Substring(6)),
                            TariffId = 263,
                            Amount = 0
                        };

                        register.Add(errRegister);
                        first = false;
                    }
                    else if(reg.ObisCode.C != 1 && reg.ObisCode.C != 2)
                    {
                        var oc = reg.ObisCode.ToHexString();
                        var errRegister = new Register()
                        {
                            ObisCode = new ObisId(oc.Substring(0, 4) + "63" + oc.Substring(6)),
                            TariffId = 63,
                            Amount = 0
                        };

                        register.Add(errRegister);
                    }
                    
                }
            }

            return register;
        }

        public static List<ushort?> GetValidDayProfilesForMeterReading(this List<DayProfile> dayProfiles, ObisId mrObisId, List<TariffStage> tariffStages)
        {
            var validDayProfiles = new List<ushort?>();
            var tariffIdList = new List<ushort?>();

            foreach (TariffStage stage in tariffStages)
            {
                var obisId = new ObisId(stage.ObisCode);

                if (mrObisId.C == obisId.C)
                {
                    tariffIdList.Add(stage.TariffNumber);
                }
            }

            foreach (DayProfile dayProfile in dayProfiles)
            {
                var isValid = true;
                foreach (DayTimeProfile dtp in dayProfile.DayTimeProfiles)
                {
                    if (!tariffIdList.Contains(dtp.TariffNumber))
                    {
                        isValid = false;
                    }
                }
                if (isValid)
                {
                    validDayProfiles.Add(dayProfile.DayId);
                }
            }

            return validDayProfiles;
        }
    }
}
