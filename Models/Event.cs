using System;

namespace FlowersFX.Models
{
    public class Event
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string locationname { get; set; }
        public string link { get; set; }
        public DateTime start_time { get; set; }
        public DateTime end_time { get; set; }
        public DateTime updated { get; set; }
    }
}