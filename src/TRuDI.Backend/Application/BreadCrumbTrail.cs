namespace TRuDI.Backend.Application
{
    using System.Collections.Generic;
    using System.Linq;

    public class BreadCrumbTrail
    {
        private readonly List<Item> items = new List<Item>();

        public IReadOnlyList<Item> Items => this.items;

        public void Add(string name, string link)
        {
            var existingItem = this.items.FirstOrDefault(i => i.Link == link);
            if (existingItem == null)
            {
                this.items.Add(new Item(this.items.Count, name, link));
            }
            else
            {
                this.BackTo(existingItem.Id);
            }
        }

        public string BackTo(int id)
        {
            if (this.items.Count > id + 1)
            {
                this.items.RemoveRange(id + 1, this.items.Count - id - 1);
            }

            return this.items.Last().Link;
        }

        public void Reset()
        {
            this.BackTo(0);
        }

        public class Item
        {
            public Item(int id, string name, string link)
            {
                this.Id = id;
                this.Name = name;
                this.Link = link;
            }

            public int Id { get; set; }

            public string Name { get; set; }

            public string Link { get; set; }
        }
    }
}
