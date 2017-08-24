namespace TRuDI.HanAdapter.XmlValidation.Models.BasicData
{
    using System;

    public class StatusFNN
    {
        public const string SMGWMASK = "00000000000000000xxx00xx000001x1";
        public const string BZMASK =   "00000000000xxxxxxxxxxxxx00000100";

        public StatusFNN(string status)
        {
            this.Status = status;
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
    }

    [FlagsAttribute]
    public enum SmgwStatusWord : uint
    {
        Identification_LSB = 1,
        Transparency_Bit = 2,
        BitPos2 = 4,
        Fatal_Error = 256,
        Systemtime_Valid = 512,
        PTB_Warning = 4096,
        PTB_Temp_Error_signed_invalid = 8192,
        PTB_Temp_Error_is_invalid = 16384,
    }

    [FlagsAttribute]
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
