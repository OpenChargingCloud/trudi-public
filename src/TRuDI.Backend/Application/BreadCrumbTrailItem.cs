namespace TRuDI.Backend.Application
{
    public class BreadCrumbTrailItem
    {
        public BreadCrumbTrailItem(int id, string name, string link)
        {
            this.Id = id;
            this.Name = name;
            this.Link = link;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Link { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item is shown on each page.
        /// </summary>
        public bool Static { get; set; }
    }
}