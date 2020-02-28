using System;
using System.Collections.Generic;

namespace FlowersFX.Models
{
    public class Menu
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public DateTime updated { get; set; }
        public DateTime downloaded { get; set; }
        public List<Section> sections { get; set; }
    }
}
