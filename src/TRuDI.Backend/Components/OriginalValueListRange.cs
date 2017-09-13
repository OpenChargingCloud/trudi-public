namespace TRuDI.Backend.Components
{
    using System.Collections.Generic;

    using TRuDI.Models;
    using TRuDI.Models.BasicData;

    public class OriginalValueListRange
    {
        public OriginalValueListRange(OriginalValueList list, IEnumerable<IntervalReading> items)
        {
            this.Source = list;
            this.Items = items;
        }

        public OriginalValueList Source { get; }
    
        public IEnumerable<IntervalReading> Items { get; }
    }
}
