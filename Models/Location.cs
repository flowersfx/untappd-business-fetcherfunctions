using System;
using System.Collections.Generic;

namespace FlowersFX.Models
{
    public class Location
    {
        public string id { get; set; }
        public DateTime downloaded { get; set; }
        public List<Event> events { get; set; }
    }
}
