using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UPS.Models;
using UPS.Services;
using UPS.ViewModels;

namespace UPS
{
    /// <summary>
    /// Interaction logic for MyUserUpdate.xaml
    /// </summary>
    public partial class MyUserUpdate : Window
    {
        private MainWindowViewModel _mainWindowViewModel;
        private readonly IEventAggregator _eventAggregator;
        private readonly IContainerProvider _containerProvider;

        //used for unsubscribing the eventaggregator
        private bool isSubscribed = false;

        public MyUserUpdate(IContainerProvider containerProvider, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _containerProvider = containerProvider;
            _eventAggregator = eventAggregator;

            // Disable the ID field
            txtId.IsEnabled = false;

            _mainWindowViewModel = _containerProvider.Resolve<MainWindowViewModel>();
            DataContext = _mainWindowViewModel;

            //subscribing
            if (!isSubscribed)
            {
                _eventAggregator.GetEvent<UserUpdatedEvent>().Subscribe(HandleUserUpdatedEvent);
                isSubscribed = true;
            }
        }

        private void HandleUserUpdatedEvent(UserEventClass MyUserEventObject)
        {
            if (MyUserEventObject.IsCompleted)
            {
                MessageBox.Show(MyUserEventObject.Message, "Success");
                this.Close();
            }
            else
            {
                MessageBox.Show(MyUserEventObject.Message, "Error");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //unsubscribing
            if (isSubscribed)
            {
                _eventAggregator.GetEvent<UserUpdatedEvent>().Unsubscribe(HandleUserUpdatedEvent);
                isSubscribed = false;
            }
        }

    }
}
