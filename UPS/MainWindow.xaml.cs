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
using System.Windows.Navigation;
using System.Windows.Shapes;
using UPS.Services;
using UPS.ViewModels;

namespace UPS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _mainWindowViewModel;
        private readonly IContainerProvider _containerProvider;
        private readonly IEventAggregator _eventAggregator;

        public MainWindow(IContainerProvider containerProvider, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            
            //resolve the viewmodel;
            _containerProvider = containerProvider;
            _mainWindowViewModel = _containerProvider.Resolve<MainWindowViewModel>();

            DataContext = _mainWindowViewModel;

            _eventAggregator = eventAggregator;
            
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            // Resolve the MyUserAdd window from the container provider.
            MyUserAdd myUserAdd = _containerProvider.Resolve<MyUserAdd>();

            // Show the MyUserAdd window.
            myUserAdd.ShowDialog();
        }

        private void UpdateUser_Click(object sender, RoutedEventArgs e)
        {
            // Check if SelectedUser is null
            if (_mainWindowViewModel.SelectedUser == null)
            {
                MessageBox.Show("No user selected.");
                return;
            }

            // Resolve the MyUserUpdate window from the container provider.
            MyUserUpdate myUserUpdate = _containerProvider.Resolve<MyUserUpdate>();

            // Open the separate view for updating the user
            myUserUpdate.ShowDialog();
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {            
            // Check if SelectedUser is null
            if (_mainWindowViewModel.SelectedUser == null)
            {
                MessageBox.Show("No user selected.");
                return;
            }

            // Resolve the UserDelete window from the container provider.
            UserDelete myUserDelete = _containerProvider.Resolve<UserDelete>();

            myUserDelete.ShowDialog();
        }

        private void SearchUser_Click(object sender, RoutedEventArgs e)
        {
            // Resolve the SearchExportWindow window from the container provider.
            SearchExportWindow mySearchUser = _containerProvider.Resolve<SearchExportWindow>();

            // Open the separate view for updating the user
            mySearchUser.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //upon loading collect the fresh data
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.OnViewLoaded();
                _eventAggregator.GetEvent<RefreshListViewEvent>().Publish();
            }
        }
    }
}
