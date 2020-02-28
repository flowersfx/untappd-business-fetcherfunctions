using System;
using System.Collections.Generic;

namespace FlowersFX.Models
{
    public class Item
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string number { get; set; }
        public string breweryname { get; set; }
        public int breweryid { get; set; }
        public string abv { get; set; }
        public string style { get; set; }
        public string type { get; set; }
        public DateTime modified { get; set; }
        public string rating { get; set; }
        public List<Container> containers { get; set; }
    }
}
