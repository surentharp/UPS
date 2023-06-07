using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using UPS.Models;
using UPS.Services;
using Prism.Services.Dialogs;
using System.Net;
using Prism.Events;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using Microsoft.Win32;

namespace UPS.ViewModels
{

    public class MainWindowViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
                private readonly ILogger _logger;
        private readonly IMyUserService _myUserService;
        private ObservableCollection<MyUser> _myUsers;
        
        private MyUser _selectedUser;
        public MyUser SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        private MyUser _newUser;
        public MyUser NewUser
        {
            get => _newUser;
            set => SetProperty(ref _newUser, value);
        }

        private bool _isUpdating;
        public bool IsUpdating
        {
            get { return _isUpdating; }
            set { SetProperty(ref _isUpdating, value); }
        }

        private bool _isAdding;
        public bool IsAdding
        {
            get { return _isAdding; }
            set { SetProperty(ref _isAdding, value); }
        }

        private List<string> _genderOptions;
        public List<string> GenderOptions
        {
            get { return _genderOptions; }
            set { SetProperty(ref _genderOptions, value); }
        }

        private List<string> _statusOptions;
        public List<string> StatusOptions
        {
            get { return _statusOptions; }
            set { SetProperty(ref _statusOptions, value); }
        }

        private SearchClass _searchUser;
        public SearchClass SearchUser
        {
            get { return _searchUser; }
            set { SetProperty(ref _searchUser, value); }
        }

        public ObservableCollection<MyUser> Users
        {
            get => _myUsers;
            set => SetProperty(ref _myUsers, value);
        }

        static string baseUrl = "https://gorest.co.in/public/v2/users?page=";

        private string _nextPageLink;
        private string _prevPageLink;
        public string _firstPageLink = $"{baseUrl}1";  // set this to your API's first page link

        public DelegateCommand AddUserCommand { get; }
        public DelegateCommand UpdateUserCommand { get; }
        public DelegateCommand DeleteUserCommand { get; }
        public DelegateCommand NextPageCommand { get; }
        public DelegateCommand PrevPageCommand { get; }
        public DelegateCommand FirstPageCommand { get; }
        public DelegateCommand LastPageCommand { get; }
        public DelegateCommand SearchAndExportCommand { get; }

        public MainWindowViewModel(IMyUserService userService, ILogger logger, IEventAggregator eventAggregator)
        {

            _logger = logger;
            _myUserService = userService;

            SearchUser = new SearchClass { Email = "", Gender = "male", Name = "", Status = "active", Filename = "" };
            NewUser = new MyUser { Email = "", Gender = "male", Name = "", Status = "active" };
            Users = new ObservableCollection<MyUser>();
            GenderOptions = new List<string> { "male", "female" };
            StatusOptions = new List<string> { "active", "inactive" };

            NextPageCommand = new DelegateCommand(NextPage, CanGoToNextPage);
            PrevPageCommand = new DelegateCommand(PrevPage, CanGoToPrevPage);
            FirstPageCommand = new DelegateCommand(FirstPage);
            LastPageCommand = new DelegateCommand(LastPage);

            AddUserCommand = new DelegateCommand(AddUser, CanAddUser);
            UpdateUserCommand = new DelegateCommand(UpdateUser, CanUpdateUser).ObservesProperty(() => SelectedUser);
            DeleteUserCommand = new DelegateCommand(DeleteUser, CanDeleteUser).ObservesProperty(() => SelectedUser);

            SearchAndExportCommand = new DelegateCommand(async () => await SearchAndExportToCSVFile(), CanSearchAndExportUser);

            _eventAggregator = eventAggregator;

        }

        //canExecuteMethod for SearchAndExportCommand
        private bool CanSearchAndExportUser()
        {
            return !String.IsNullOrEmpty(SearchUser.Filename);
        }

        //eventAggregator for refreshing the listview in the main window
        public void OnViewLoaded()
        {
            _eventAggregator.GetEvent<RefreshListViewEvent>().Subscribe(RefreshListView);
        }

        //Method for loading users on given URL, can be used for nextpage, previouspage, firstpage and lastpage
        public async void LoadUsers(string url)
        {
            try
            {
                var response = await _myUserService.GetUsersByUrl(url);
                Users = new ObservableCollection<MyUser>(response.Users);

                _nextPageLink = response.NextPageUrl;
                _prevPageLink = response.PreviousPageUrl;

                NextPageCommand.RaiseCanExecuteChanged();
                PrevPageCommand.RaiseCanExecuteChanged();

                Log.Information("users loaded");
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error loading users: {e.Message}");
            }
        }

        //canExecuteMethod for AddUserCommand
        private bool CanAddUser()
        {
            return NewUser != null;
        }

        //Method for adding users
        public async void AddUser()
        {
            //boolean for controlling the visiblity of button control in the User Adding Window
            IsAdding = true;

            try
            {
                //calling the api service
                var addUserResponse = await _myUserService.AddUser(NewUser);

                if (addUserResponse.StatusCode == HttpStatusCode.Created && addUserResponse.User != null)
                {
                    //adding in the collection
                    Users.Add(addUserResponse.User);
                    Log.Information("User added successfully");

                    //letting know the view about the status
                    _eventAggregator.GetEvent<UserAddedEvent>().Publish(new UserEventClass { Message = "User added successfully",  IsCompleted = true, UserData = new MyUser { Email = NewUser.Email, Gender = NewUser.Gender, Id = 0, Name = NewUser.Name, Status = NewUser.Status } });
                    
                    //reseting the object
                    NewUser = new MyUser { Email = "", Gender = "male", Name = "", Status = "active" };
                }
                else
                {//letting know the view about the status
                    _eventAggregator.GetEvent<UserAddedEvent>().Publish(new UserEventClass { Message = $"Failed to add user. Status code: {addUserResponse.StatusCode}", IsCompleted = false, UserData = new MyUser { Email = NewUser.Email, Gender = NewUser.Gender, Id = 0, Name = NewUser.Name, Status = NewUser.Status } });
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error adding users: {e.Message}");

                //letting know the view about the status
                _eventAggregator.GetEvent<UserAddedEvent>().Publish(new UserEventClass { Message = $"Error adding users: {e.Message}", IsCompleted = false, UserData = new MyUser { Email = NewUser.Email, Gender = NewUser.Gender, Id = 0, Name = NewUser.Name, Status = NewUser.Status } });

            }
            finally
            {
                IsAdding = false;
            }
        }

        //canExecuteMethod for UpdateUserCommand
        private bool CanUpdateUser()
        {
            return SelectedUser != null;
        }

        //Method for updating users
        private async void UpdateUser()
        {
            //boolean for controlling the visiblity of button control in the User Updating Window
            IsUpdating = true;

            try
            {

                if(SelectedUser == null)
                {
                    return;
                }

                //calling the api service
                var addUserResponse = await _myUserService.UpdateUser(SelectedUser);

                if (addUserResponse.StatusCode == HttpStatusCode.OK && addUserResponse.User != null)
                {
                    Users.Remove(SelectedUser);
                    Users.Add(addUserResponse.User);

                    Log.Information("User updated successfully");

                    //letting know the view about the status
                    _eventAggregator.GetEvent<UserUpdatedEvent>().Publish(new UserEventClass { Message = $"User updated successfully", IsCompleted = true, UserData = new MyUser { Email = NewUser.Email, Gender = NewUser.Gender, Id = 0, Name = NewUser.Name, Status = NewUser.Status } });

                }
                else
                {
                    //letting know the view about the status
                    _eventAggregator.GetEvent<UserUpdatedEvent>().Publish(new UserEventClass { Message = $"Failed to update user. Status code: {addUserResponse.StatusCode}", IsCompleted = false, UserData = new MyUser { Email = NewUser.Email, Gender = NewUser.Gender, Id = 0, Name = NewUser.Name, Status = NewUser.Status } });

                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error update users: {e.Message}");

                //letting know the view about the status
                _eventAggregator.GetEvent<UserUpdatedEvent>().Publish(new UserEventClass { Message = $"Error update users: {e.Message}", IsCompleted = false, UserData = new MyUser { Email = NewUser.Email, Gender = NewUser.Gender, Id = 0, Name = NewUser.Name, Status = NewUser.Status } });

            }
            finally
            {
                IsUpdating = false;
            }

        }

        //canExecuteMethod for DeleteUserCommand
        private bool CanDeleteUser()
        {
            return SelectedUser != null;
        }

        //Method for deleting users
        private async void DeleteUser()
        {

            // Delete the item.
            try
            {
                //calling the api service
                var deleteUserResponse = await _myUserService.DeleteUser(SelectedUser.Id);

                if (deleteUserResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    //letting know the view about the status
                    _eventAggregator.GetEvent<UserDeletedEvent>().Publish(new UserEventClass { Message = $"User deleted", IsCompleted = true, UserData = new MyUser { Email = SelectedUser.Email, Gender = SelectedUser.Gender, Id = SelectedUser.Id, Name = SelectedUser.Name, Status = SelectedUser.Status } });

                    Users.Remove(SelectedUser);

                    Log.Information("User deleted");
                }
                else
                {
                    //letting know the view about the status
                    _eventAggregator.GetEvent<UserDeletedEvent>().Publish(new UserEventClass { Message = $"User not deleted, there might be an error", IsCompleted = false, UserData = new MyUser { Email = SelectedUser.Email, Gender = SelectedUser.Gender, Id = SelectedUser.Id, Name = SelectedUser.Name, Status = SelectedUser.Status } });
                }

            }
            catch (Exception e)
            {
                //MessageBox.Show($"Error deleting user: {e.Message}");
                Log.Error(e, $"Error deleting user: {e.Message}");

                //letting know the view about the status
                _eventAggregator.GetEvent<UserDeletedEvent>().Publish(new UserEventClass { Message = $"Error deleting user: {e.Message}", IsCompleted = true, UserData = new MyUser { Email = SelectedUser.Email, Gender = SelectedUser.Gender, Id = SelectedUser.Id, Name = SelectedUser.Name, Status = SelectedUser.Status } });

            }

        }

        //Navigation methods for next page
        private void NextPage()
        {
            if (_nextPageLink != null && _nextPageLink != "")
            {
                LoadUsers(_nextPageLink);
            }
        }

        //canExecuteMethod for NextPageCommand
        private bool CanGoToNextPage()
        {
            return (_nextPageLink != null && _nextPageLink != "");
        }

        //Navigation methods for previous page
        private void PrevPage()
        {
            if (_prevPageLink != null && _prevPageLink != "")
            {
                LoadUsers(_prevPageLink);
            }
        }

        //canExecuteMethod for PrevPageCommand
        private bool CanGoToPrevPage()
        {
            return (_prevPageLink != null && _prevPageLink != "");
        }

        //Navigation methods for first page
        private void FirstPage()
        {
            LoadUsers(_firstPageLink);
        }

        //Navigation methods for last page
        private async void LastPage()
        {
            var response = await _myUserService.GetUsersByUrl(baseUrl);
            Users = new ObservableCollection<MyUser>(response.Users);

            _nextPageLink = response.NextPageUrl;
            _prevPageLink = response.PreviousPageUrl;

            int totalNumberOfPages = response.TotalPages;

            string lastPageUrl = baseUrl + totalNumberOfPages.ToString();

            // Load users from the last page.
            LoadUsers(lastPageUrl);
        }

        //Refresh the collected data, so it will refresh the listview
        private void RefreshListView()
        {
            LoadUsers(_firstPageLink);
        }

        //Method for the SearchAndExportCommand
        public async Task SearchAndExportToCSVFile()
            {
            try
            {
                //calling the api service
                await _myUserService.SearchAndExportToCSVFile(SearchUser?.Filename, SearchUser?.Name, SearchUser?.Id, SearchUser?.Email, SearchUser?.Gender, SearchUser?.Status);

                //letting know the view about the status
                _eventAggregator.GetEvent<UserSearchEvent>().Publish(new UserEventClass { Message = $"Search results exported to CSV file successfully!", IsCompleted = true, UserData = new MyUser { Email = SearchUser.Email, Gender = SearchUser.Gender, Id = SearchUser.Id, Name = SearchUser.Name, Status = SearchUser.Status } });

                Log.Error($"Search results exported to CSV file successfully!");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error exporting search results: {ex.Message}");

                //letting know the view about the status
                _eventAggregator.GetEvent<UserSearchEvent>().Publish(new UserEventClass { Message = $"Error exporting search results: {ex.Message}", IsCompleted = false, UserData = new MyUser { Email = SearchUser.Email, Gender = SearchUser.Gender, Id = SearchUser.Id, Name = SearchUser.Name, Status = SearchUser.Status } });

            }
        }
    }
}
