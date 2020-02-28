using System.Collections.Generic;

namespace FlowersFX.Models
{
    public class Section
    {
        public int id { get; set; }
        public string name { get; set; }
        public int position { get; set; }
        public string description { get; set; }
        public List<Item> items {get; set; }
    }
}
