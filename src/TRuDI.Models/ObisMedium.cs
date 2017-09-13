namespace TRuDI.Models
{
    public enum ObisMedium : byte
    {
        Abstract = 0x00,
        Electricity = 0x01,
        HeatCostAllocator = 0x04,
        Cooling = 0x05,
        Heat = 0x06,
        Gas = 0x07,
        WaterCold = 0x08,
        WaterHot = 0x09,

        Communication = 0x0E,
    }
}
