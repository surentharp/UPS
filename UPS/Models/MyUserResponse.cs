using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPS.Models
{
    //Used for storing the response from the API
    public class MyUserResponse
    {
        public IEnumerable<MyUser> Users { get; set; }
        public string NextPageUrl { get; set; }
        public string PreviousPageUrl { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

    }
}
