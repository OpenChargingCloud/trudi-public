namespace TRuDI.TafAdapter.Interface
{
    using System;

    public class TafAdapterData
    {
        public TafAdapterData(Type summaryView, Type detailView, ITafData data)
        {
            this.SummaryView = summaryView;
            this.DetailView = detailView;
            this.Data = data;
        }

        public Type SummaryView { get; }

        public Type DetailView { get; }

        public ITafData Data { get; }
    }
}