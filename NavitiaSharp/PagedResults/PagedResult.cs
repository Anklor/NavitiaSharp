
using NavitiaSharp.Deserializers;
using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavitiaSharp
{

    public class PagedResult<T> where T : new()
    {
        [DeserializeAs(Name = "pagination")]
        public Pagination Pagination { get; set; }

        [PagedResultData]
        public List<T> Results { get; set; }

        [DeserializeAs(Name = "disruptions")]
        public List<object> Disruptions { get; set; }

        [DeserializeAs(Name = "links")]
        public List<Link> Links { get; set; }

        public bool HasMoreData
        {
            get
            {
                int maxPage = (int)Math.Floor((float)Pagination.TotalResult / Pagination.ItemsPerPage);
                bool hasData = (Pagination.StartPage) < maxPage;
                return hasData;
            }
        }
    }

    public class Pagination
    {
        [DeserializeAs(Name = "start_page")]
        public int StartPage { get; set; }

        [DeserializeAs(Name = "items_on_page")]
        public int ItemsOnPage { get; set; }

        [DeserializeAs(Name = "items_per_page")]
        public int ItemsPerPage { get; set; }

        [DeserializeAs(Name = "total_result")]
        public int TotalResult { get; set; }
    }

    public class Link
    {
        [DeserializeAs(Name = "href")]
        public string Href { get; set; }

        [DeserializeAs(Name = "type")]
        public string Type { get; set; }

        [DeserializeAs(Name = "rel")]
        public string Rel { get; set; }

        [DeserializeAs(Name = "templated")]
        public bool Templated { get; set; }
    }



}
