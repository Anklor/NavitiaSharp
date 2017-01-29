using NavitiaSharp;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SncfOpenData
{
    public class SncfApi : NavitiaApi
    {
        const string BaseUrl = "https://api.sncf.com/v1/coverage/sncf";


        public SncfApi(string devKey)
             : base(BaseUrl, devKey)
        {
        }

       
    }

}
