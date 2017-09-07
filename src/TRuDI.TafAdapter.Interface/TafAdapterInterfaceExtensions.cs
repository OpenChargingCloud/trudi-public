namespace TRuDI.TafAdapter.Interface
{
    using System;
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

            if (supplier.AnalysisProfile.TariffUseCase == HanAdapter.Interface.TafId.Taf2)
            {
                var errorRegister = register.Where(r => r.ObisCode.E == 63).ToList() ?? new List<Register>();

                if (errorRegister.Count > 0)
                {
                    foreach (Register reg in errorRegister)
                    {
                        reg.TariffId = 63;
                    }
                }
                else
                {
                    var first = true;
                    var seccond = true;
                    var next = 2;
                    foreach (Register reg in register.ToList())
                    {
                        if (first && reg.ObisCode.C == 1)
                        {
                            var oc = reg.ObisCode.ToHexString();
                            var errRegister = new Register()
                            {
                                ObisCode = new ObisId(oc) { E = 63 },
                                TariffId = 63,
                                Amount = 0
                            };

                            register.Add(errRegister);
                            first = false;
                        }
                        else if (seccond && reg.ObisCode.C == 2)
                        {
                            var oc = reg.ObisCode.ToHexString();
                            var errRegister = new Register()
                            {
                                ObisCode = new ObisId(oc) { E = 63 },
                                TariffId = 63,
                                Amount = 0
                            };

                            register.Add(errRegister);
                            seccond = false;
                        }
                        else if (reg.ObisCode.C != 1 && reg.ObisCode.C != 2 && reg.ObisCode.C > next)
                        {
                            var oc = reg.ObisCode.ToHexString();
                            var errRegister = new Register()
                            {
                                ObisCode = new ObisId(oc) { E = 63 },
                                TariffId = 63,
                                Amount = 0
                            };
                            next++;
                            register.Add(errRegister);
                        }

                    }
                }
            }

            return register;
        }

        public static List<ushort?> GetValidDayProfilesForMeterReading(this List<DayProfile> dayProfiles, ObisId mrObisId, List<TariffStage> tariffStages)
        {
            var validDayProfiles = new List<ushort?>();
            var tariffIdList = new List<ushort?>();

            if (mrObisId == default(ObisId) || tariffStages == null)
            {
                throw new ArgumentNullException("The parameter mrObisId or tariffStages is null.");
            }

            if(tariffStages.Count < 1 || dayProfiles.Count < 1)
            {
                throw new ArgumentException("The parameter tariffStages or the parameter dayProfiles contains 0 elements.");
            }

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
