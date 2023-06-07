using Moq;
using NUnit.Framework;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UPS.Models;
using UPS.Services;
using UPS.ViewModels;

namespace TestProject1
{
    [TestFixture]
    public class MainWindowViewModelTests
    {
        private MainWindowViewModel _viewModel;
        private Mock<IDialogService> _dialogServiceMock;
        private Mock<IMyUserService> _userServiceMock;
        private Mock<IEventAggregator> _eventAggregatorMock;
        private Mock<Serilog.ILogger> _logger;

        [SetUp]
        public void Setup()
        {
            
            _userServiceMock = new Mock<IMyUserService>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _logger = new Mock<Serilog.ILogger>();

            _viewModel = new MainWindowViewModel(_userServiceMock.Object, _logger.Object, _eventAggregatorMock.Object);
        }

        [Test]
        public void AddUserCommand_WhenExecutedWithSuccess_PublishesUserAddedEvent()
        {
            // Arrange
            var newUser = new MyUser { Email = "test@example.com", Gender = "male", Name = "Suren", Status = "active" };
            _viewModel.NewUser = newUser;

            var addUserResponse = new ServerResponse { StatusCode = HttpStatusCode.Created, User = newUser };
            _userServiceMock.Setup(x => x.AddUser(newUser)).ReturnsAsync(addUserResponse);

            bool userAddedEventPublished = false;
            _eventAggregatorMock.Setup(x => x.GetEvent<UserAddedEvent>().Publish(It.IsAny<UserEventClass>()))
                .Callback(() => userAddedEventPublished = true);

            // Act
            _viewModel.AddUserCommand.Execute();

            // Assert
            Assert.IsTrue(userAddedEventPublished);
        }

        [Test]
        public void AddUserCommand_WhenExecutedWithFailure_PublishesUserAddedEvent()
        {
            // Arrange
            var newUser = new MyUser { Email = "test@example.com", Gender = "male", Name = "Suren", Status = "active" };
            _viewModel.NewUser = newUser;

            var addUserResponse = new ServerResponse { StatusCode = HttpStatusCode.BadRequest, User = null };
            _userServiceMock.Setup(x => x.AddUser(newUser)).ReturnsAsync(addUserResponse);

            bool userAddedEventPublished = false;
            _eventAggregatorMock.Setup(x => x.GetEvent<UserAddedEvent>().Publish(It.IsAny<UserEventClass>()))
                .Callback(() => userAddedEventPublished = true);

            // Act
            _viewModel.AddUserCommand.Execute();

            // Assert
            Assert.IsTrue(userAddedEventPublished);
        }

        [Test]
        public void UpdateUserCommand_ValidUser_UserUpdatedSuccessfully()
        {
            // Arrange
            var selectedUser = new MyUser
            {
                Id = 1,
                Name = "Suren",
                Email = "Suren@example.com",
                Gender = "male",
                Status = "active"
            };

            _viewModel.SelectedUser = selectedUser;

            _userServiceMock.Setup(x => x.UpdateUser(selectedUser))
                .ReturnsAsync(new ServerResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    User = selectedUser // Updated user object
                });

            // Act
            _viewModel.UpdateUserCommand.Execute();

            // Assert
            _userServiceMock.Verify(x => x.UpdateUser(selectedUser), Times.Once);
            Assert.IsFalse(_viewModel.IsUpdating);
            Assert.That(_viewModel.Users.Single(), Is.EqualTo(selectedUser));
        }

        [Test]
        public void UpdateUserCommand_InvalidUser_FailedToUpdateUser()
        {
            // Arrange
            var selectedUser = new MyUser
            {
                Id = 1,
                Name = "Suren",
                Email = "Suren@example.com",
                Gender = "male",
                Status = "active"
            };

            _viewModel.SelectedUser = selectedUser;

            _userServiceMock.Setup(x => x.UpdateUser(selectedUser))
                .ReturnsAsync(new ServerResponse
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act
            _viewModel.UpdateUserCommand.Execute();

            // Assert
            _userServiceMock.Verify(x => x.UpdateUser(selectedUser), Times.Once);
            Assert.IsFalse(_viewModel.IsUpdating);
            Assert.That(_viewModel.Users.Count, Is.EqualTo(0));

        }

        [Test]
        public void UpdateUserCommand_SelectedUserIsNull_NoActionTaken()
        {
            // Arrange
            _viewModel.SelectedUser = null;

            // Act
            _viewModel.UpdateUserCommand.Execute();

            // Assert
            _userServiceMock.Verify(x => x.UpdateUser(It.IsAny<MyUser>()), Times.Never);
            Assert.IsFalse(_viewModel.IsUpdating);

        }

        [Test]
        public void UpdateUserCommand_ExceptionThrown_ErrorHandled()
        {
            // Arrange
            var selectedUser = new MyUser
            {
                Id = 1,
                Name = "Suren",
                Email = "Suren@example.com",
                Gender = "male",
                Status = "active"
            };

            _viewModel.SelectedUser = selectedUser;

            _userServiceMock.Setup(x => x.UpdateUser(selectedUser))
                .ThrowsAsync(new Exception("Something went wrong"));

            // Act
            _viewModel.UpdateUserCommand.Execute();

            // Assert
            _userServiceMock.Verify(x => x.UpdateUser(selectedUser), Times.Once);
            Assert.IsFalse(_viewModel.IsUpdating);

        }

        [Test]
        public async Task DeleteUserCommand_WhenExecuted_DeletesUserAndUpdatesCollection()
        {
            // Arrange
            var userToDelete = new MyUser { Id = 123 };
            _viewModel.SelectedUser = userToDelete;

            var deleteUserResponse = new ServerResponse { StatusCode = HttpStatusCode.NoContent };
            _userServiceMock.Setup(x => x.DeleteUser(userToDelete.Id)).ReturnsAsync(deleteUserResponse);

            // Act
            _viewModel.DeleteUserCommand.Execute();

            // Assert
            _userServiceMock.Verify(x => x.DeleteUser(userToDelete.Id), Times.Once);
            Assert.IsFalse(_viewModel.Users.Contains(userToDelete));
        }

    }
}
