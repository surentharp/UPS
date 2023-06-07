using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPS.Models
{
    //Used for EventAggregator between view and viewmodel
    public class UserEventClass
    {
        public MyUser? UserData { get; set; }
        public bool IsCompleted { get; set; }
        public string Message { get; set; }
    }
}
