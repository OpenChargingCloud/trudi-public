namespace TRuDI.Backend.Application
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// This class is used to manage the bread crumb trail navigation on top of the page.
    /// </summary>
    public class BreadCrumbTrail
    {
        private readonly List<BreadCrumbTrailItem> items = new List<BreadCrumbTrailItem>();

        public IReadOnlyList<BreadCrumbTrailItem> Items => this.items;

        public void Add(string name, string link, bool removeFollowingItems)
        {
            var existingItem = this.items.FirstOrDefault(i => i.Link == link);
            if (existingItem == null)
            {
                foreach (var item in this.items)
                {
                    item.IsActive = true;
                    item.IsSelected = false;
                }

                this.items.Add(new BreadCrumbTrailItem(this.items.Count, name, link) { IsSelected = true });
            }
            else
            {
                this.BackTo(existingItem.Id, removeFollowingItems);
            }
        }

        public void RemoveUnselectedItems()
        {
            this.items.RemoveAll(i => !(i.IsActive || i.IsSelected));
        }

        public string BackTo(int id, bool removeFollowingItems)
        {
            if (removeFollowingItems && this.items.Count > id + 1)
            {
                this.items.RemoveRange(id + 1, this.items.Count - id - 1);

                for (int i = 0; i < this.items.Count; i++)
                {
                    this.items[i].IsActive = true;
                    this.Items[i].IsSelected = i == id;
                }

                return this.items.Last().Link;
            }

            for (int i = 0; i < this.items.Count; i++)
            {
                this.items[i].IsActive = i < id;
                this.Items[i].IsSelected = i == id;
            }

            return this.items[id].Link;
        }

        public void Reset()
        {
            this.BackTo(0, true);
        }
    }
}
