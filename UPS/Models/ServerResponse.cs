using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UPS.Models
{
    //fetching data from the api service to the viewmodel
    public class ServerResponse
    {
        public MyUser User { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
