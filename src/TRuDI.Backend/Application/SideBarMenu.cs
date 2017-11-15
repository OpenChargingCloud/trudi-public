namespace TRuDI.Backend.Application
{
    using System.Collections.Generic;

    public class SideBarMenu
    {
        private readonly List<BreadCrumbTrailItem> items = new List<BreadCrumbTrailItem>();

        public IReadOnlyList<BreadCrumbTrailItem> Items => this.items;

        public void Add(string name, string link, bool staticItem = false, bool useOnClick = false)
        {
            this.items.Add(new BreadCrumbTrailItem(this.items.Count, name, link) { Static = staticItem, UseOnClick = useOnClick });
        }

        public void Clear()
        {
            for (int i = this.items.Count - 1; i >= 0; i--)
            {
                if (!this.items[i].Static)
                {
                    this.items.RemoveAt(i);
                }
            }
        }
    }
}