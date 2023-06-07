using Microsoft.Win32;
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
    /// Interaction logic for SearchExportWindow.xaml
    /// </summary>
    public partial class SearchExportWindow : Window
    {
        private MainWindowViewModel _mainWindowViewModel;
        private readonly IEventAggregator _eventAggregator;
        private readonly IContainerProvider _containerProvider;

        //used for unsubscribing the eventaggregator
        private bool isSubscribed = false;

        public SearchExportWindow(IContainerProvider containerProvider, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _containerProvider = containerProvider;
            _eventAggregator = eventAggregator;

            _mainWindowViewModel = _containerProvider.Resolve<MainWindowViewModel>();
            DataContext = _mainWindowViewModel;

            //subscribing
            if (!isSubscribed)
            {
                _eventAggregator.GetEvent<UserSearchEvent>().Subscribe(HandleUserSearchEvent);
                isSubscribed = true;
            }
        }

        private void HandleUserSearchEvent(UserEventClass MyUserEventObject)
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

        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                lblFilePath.Content = filePath;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWindowViewModel.SearchAndExportCommand.CanExecute())
            {
                _mainWindowViewModel.SearchAndExportCommand.Execute();

            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //unsubscribing
            if (isSubscribed)
            {
                _eventAggregator.GetEvent<UserSearchEvent>().Unsubscribe(HandleUserSearchEvent);
                isSubscribed = false;
            }

        }
    }
}
