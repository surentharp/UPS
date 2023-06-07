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
    /// Interaction logic for UserDelete.xaml
    /// </summary>
    public partial class UserDelete : Window
    {
        private MainWindowViewModel _mainWindowViewModel;
        private readonly IEventAggregator _eventAggregator;
        private readonly IContainerProvider _containerProvider;

        //used for unsubscribing the eventaggregator
        private bool isSubscribed = false;

        public UserDelete(IContainerProvider containerProvider, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _containerProvider = containerProvider;
            _eventAggregator = eventAggregator;

            _mainWindowViewModel = _containerProvider.Resolve<MainWindowViewModel>();
            DataContext = _mainWindowViewModel;

            //subscribing
            if (!isSubscribed)
            {
                _eventAggregator.GetEvent<UserDeletedEvent>().Subscribe(HandleUserDeletedEvent);
                isSubscribed = true;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWindowViewModel.DeleteUserCommand.CanExecute())
            {
                if (MessageBox.Show("Are you sure to delete?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _mainWindowViewModel.DeleteUserCommand.Execute();
                }
            }
        }

        private void HandleUserDeletedEvent(UserEventClass MyUserEnventObject)
        {
            if (MyUserEnventObject.IsCompleted)
            {
                MessageBox.Show(MyUserEnventObject.Message, "Success");
                this.Close();
            }
            else
            {
                MessageBox.Show(MyUserEnventObject.Message, "Error");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //unsubscribing
            if (isSubscribed)
            {
                _eventAggregator.GetEvent<UserDeletedEvent>().Unsubscribe(HandleUserDeletedEvent);
                isSubscribed = false;
            }

            //upon closing refresh the main window listview
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.OnViewLoaded();
                _eventAggregator.GetEvent<RefreshListViewEvent>().Publish();
            }
        }
    }
}
