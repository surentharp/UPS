using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UPS.Models;

namespace UPS.Services
{
    //used for refreshing the user collection
    public class RefreshListViewEvent : PubSubEvent
    {
    }

    //used for fetching data from viewmodel to add window
    public class UserAddedEvent : PubSubEvent<UserEventClass>
    {

    }

    //used for fetching data from viewmodel to update window
    public class UserUpdatedEvent : PubSubEvent<UserEventClass>
    {

    }

    //used for fetching data from viewmodel to delete window
    public class UserDeletedEvent : PubSubEvent<UserEventClass>
    {

    }

    //used for fetching data from viewmodel to search window
    public class UserSearchEvent : PubSubEvent<UserEventClass>
    {

    }
}
