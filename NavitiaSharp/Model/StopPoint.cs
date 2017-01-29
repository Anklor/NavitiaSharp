﻿using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{
    public class StopPoint
    {

        [DeserializeAs(Name = "name")]
        public string Name { get; set; }

        [DeserializeAs(Name = "links")]
        public List<Link> Links { get; set; }

        [DeserializeAs(Name = "coord")]
        public Coord Coord { get; set; }

        [DeserializeAs(Name = "label")]
        public string Label { get; set; }

        [DeserializeAs(Name = "equipments")]
        public List<object> Equipments { get; set; }

        [DeserializeAs(Name = "administrative_regions")]
        public List<AdministrativeRegion> AdministrativeRegions { get; set; }

        [DeserializeAs(Name = "id")]
        public string Id { get; set; }

        [DeserializeAs(Name = "stop_area")]
        public StopArea StopArea { get; set; }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
