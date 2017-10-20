namespace TRuDI.Models.BasicData
{
    using System;

    public class StatusFNN
    {
        public const string SMGWMASK = "00000000000000000xxx00xx000001x1";
        public const string BZMASK = "00000000000xxxxxxxxxxxxx00000100";

        public StatusFNN(string status)
        {
            this.Status = status.PadLeft(16, '0');
            var smgwStat = Convert.ToInt64(this.Status.Substring(0, 8), 16);
            var bzStat = Convert.ToInt64(this.Status.Substring(8), 16);
            this.SmgwStatusWord = (SmgwStatusWord)smgwStat;
            this.BzStatusWord = (BzStatusWord)bzStat;
        }

        public string Status
        {
            get; set;
        }

        public SmgwStatusWord SmgwStatusWord
        {
            get; set;
        }

        public BzStatusWord BzStatusWord
        {
            get; set;
        }

        public StatusPTB MapToStatusPtb()
        {
            if (this.BzStatusWord.HasFlag(BzStatusWord.Fatal_Error) ||
                this.SmgwStatusWord.HasFlag(SmgwStatusWord.Fatal_Error))
            {
                return StatusPTB.Fatal_Error;
            }

            if (this.SmgwStatusWord.HasFlag(SmgwStatusWord.Systemtime_Invalid) ||
                this.SmgwStatusWord.HasFlag(SmgwStatusWord.PTB_Temp_Error_is_invalid))
            {
                return StatusPTB.Temp_Error_is_invalid;
            }

            if (this.SmgwStatusWord.HasFlag(SmgwStatusWord.PTB_Temp_Error_signed_invalid))
            {
                return StatusPTB.Warning;
            }

            return StatusPTB.No_Error;
        }
    }

    [Flags]
    public enum SmgwStatusWord : uint
    {
        Identification_LSB = 1,
        Transparency_Bit = 2,
        BitPos2 = 4,
        Fatal_Error = 256,
        Systemtime_Invalid = 512,
        PTB_Warning = 4096,
        PTB_Temp_Error_signed_invalid = 8192,
        PTB_Temp_Error_is_invalid = 16384,
    }

    [Flags]
    public enum BzStatusWord : uint
    {
        BitPos2 = 4,
        Start_Up = 256,
        Magnetically_Influenced = 512,
        Manipulation_KD_PS = 1024,
        Sum_Energiedirection_neg = 2048,
        Energiedirection_L1_neg = 4096,
        Energiedirection_L2_neg = 8192,
        Energiedirection_L3_neg = 16384,
        PhaseSequenz_RotatingField_Not_L1_L2_L3 = 32768,
        BackStop_Active = 65536,
        Fatal_Error = 131072,
        Lead_Voltage_L1_existent = 262144,
        Lead_Voltage_L2_existent = 524288,
        Lead_Voltage_L3_existent = 1048576
    }
}
