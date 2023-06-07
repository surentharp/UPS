using Moq;
using Moq.Protected;
using Prism.Services.Dialogs;
using System.Net;
using System.Text.Json;
using System.Text;
using System.Windows;
using UPS.Models;
using UPS.Services;
using UPS.ViewModels;

namespace TestProject1
{
    [TestFixture]
    public class MyUserServiceTests
    {
        private MyUserService _userService;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;

        [SetUp]
        public void Setup()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://gorest.co.in/public/v2/")
            };

            _userService = new MyUserService(_httpClient, new Mock<Serilog.ILogger>().Object);
        }


        [Test]
        public async Task AddUser_ValidUser_ReturnsServerResponseWithUser()
        {
            // Arrange
            var newUser = new MyUser
            {
                Name = "Suren doe",
                Email = "Suren.doe@example.com",
                Gender = "male",
                Status = "active"
            };

            var expectedResponse = new ServerResponse
            {
                StatusCode = HttpStatusCode.Created,
                User = newUser
            };

            //var options = new JsonSerializerOptions
            //{
            //    PropertyNamingPolicy = new LowercaseNamingPolicy()
            //};
            var serializedUser = JsonSerializer.Serialize(newUser);
            var content = new StringContent(serializedUser, Encoding.UTF8, "application/json");

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(serializedUser)
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _userService.AddUser(newUser);

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(expectedResponse.StatusCode));
            Assert.That(result.User.Name, Is.EqualTo(expectedResponse.User.Name));
            Assert.That(result.User.Email, Is.EqualTo(expectedResponse.User.Email));
            Assert.That(result.User.Gender, Is.EqualTo(expectedResponse.User.Gender));
            Assert.That(result.User.Status, Is.EqualTo(expectedResponse.User.Status));
        }

        [Test]
        public void AddUser_NullUser_ThrowsArgumentNullException()
        {
            // Arrange
            MyUser newUser = null;

            // Act and Assert
            Assert.ThrowsAsync<NullReferenceException>(async () => await _userService.AddUser(newUser));
        }

        [Test]
        public void AddUser_InvalidName_ThrowsArgumentException()
        {
            // Arrange
            var newUser = new MyUser
            {
                Name = "   ", // Invalid name
                Email = "Suren.doe@example.com",
                Gender = "male",
                Status = "active"
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _userService.AddUser(newUser));
        }

        [Test]
        public void AddUser_InvalidEmail_ThrowsArgumentException()
        {
            // Arrange
            var newUser = new MyUser
            {
                Name = "Suren doe",
                Email = "invalid-email", // Invalid email format
                Gender = "male",
                Status = "active"
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _userService.AddUser(newUser));
        }

        [Test]
        public void AddUser_InvalidGender_ThrowsArgumentException()
        {
            // Arrange
            var newUser = new MyUser
            {
                Name = "Suren doe",
                Email = "Suren.doe@example.com",
                Gender = "other", // Invalid gender
                Status = "active"
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _userService.AddUser(newUser));
        }

        [Test]
        public void AddUser_InvalidStatus_ThrowsArgumentException()
        {
            // Arrange
            var newUser = new MyUser
            {
                Name = "Suren doe",
                Email = "Suren.doe@example.com",
                Gender = "male",
                Status = "pending" // Invalid status
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _userService.AddUser(newUser));
        }

        [Test]
        public async Task UpdateUser_ValidUser_ReturnsSuccessResponse()
        {
            // Arrange
            var user = new MyUser
            {
                Id = 1,
                Name = "Suren",
                Email = "Suren@example.com",
                Gender = "male",
                Status = "active"
            };
            var updatedUser = new MyUser
            {
                Id = 1,
                Name = "Suren Doe",
                Email = "Suren.doe@example.com",
                Gender = "male",
                Status = "inactive"
            };
            var serverResponse = new ServerResponse
            {
                StatusCode = HttpStatusCode.OK,
                User = updatedUser
            };
            var serializedUser = "{\"id\":1,\"name\":\"Suren Doe\",\"email\":\"Suren.doe@example.com\",\"gender\":\"male\",\"status\":\"inactive\"}";
            var content = new StringContent(serializedUser, Encoding.UTF8, "application/json");
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _userService.UpdateUser(user);

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.User.Id, Is.EqualTo(updatedUser.Id));
            Assert.That(result.User.Name, Is.EqualTo(updatedUser.Name));
            Assert.That(result.User.Email, Is.EqualTo(updatedUser.Email));
            Assert.That(result.User.Gender, Is.EqualTo(updatedUser.Gender));
            Assert.That(result.User.Status, Is.EqualTo(updatedUser.Status));
        }

        [Test]
        public void UpdateUser_InvalidUser_ThrowsArgumentException()
        {
            // Arrange
            var user = new MyUser
            {
                Id = 1,
                Name = "", // Invalid: empty name
                Email = "Suren@example.com",
                Gender = "male",
                Status = "active"
            };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _userService.UpdateUser(user));
        }

        [Test]
        public void UpdateUser_InvalidName_ThrowsArgumentException()
        {
            // Arrange
            var user = new MyUser
            {
                Id = 1,
                Name = "", // Invalid: empty name
                Email = "Suren@example.com",
                Gender = "male",
                Status = "active"
            };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _userService.UpdateUser(user));
        }

        [Test]
        public void UpdateUser_InvalidEmail_ThrowsArgumentException()
        {
            // Arrange
            var user = new MyUser
            {
                Id = 1,
                Name = "Suren",
                Email = "Surenexample.com", // Invalid: missing @ symbol
                Gender = "male",
                Status = "active"
            };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _userService.UpdateUser(user));
        }

        [Test]
        public void UpdateUser_InvalidGender_ThrowsArgumentException()
        {
            // Arrange
            var user = new MyUser
            {
                Id = 1,
                Name = "Suren",
                Email = "Suren@example.com",
                Gender = "other", // Invalid: unsupported gender value
                Status = "active"
            };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _userService.UpdateUser(user));
        }

        [Test]
        public void UpdateUser_InvalidStatus_ThrowsArgumentException()
        {
            // Arrange
            var user = new MyUser
            {
                Id = 1,
                Name = "Suren",
                Email = "Suren@example.com",
                Gender = "male",
                Status = "unknown" // Invalid: unsupported status value
            };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _userService.UpdateUser(user));
        }

        [Test]
        public async Task DeleteUser_WhenExecuted_DeletesUser()
        {
            // Arrange
            int userId = 1;
            var expectedUrl = $"{_httpClient.BaseAddress}users/{userId}";

            // Set up a sample response
            var response = new HttpResponseMessage(HttpStatusCode.NoContent);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var serverResponse = await _userService.DeleteUser(userId);

            // Assert
            _httpMessageHandlerMock.Protected().Verify<Task<HttpResponseMessage>>("SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>());

            Assert.That(serverResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.IsNull(serverResponse.User);
        }

        [Test]
        public async Task SearchAndExportCommand_WhenCalled_ExportsUsersToCSVFile()
        {
            // Arrange
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://gorest.co.in/public/v2/")
            };

            var userService = new MyUserService(httpClient, new Mock<Serilog.ILogger>().Object);

            var filePath = "test_file.csv";
            var searchUser = new MyUser
            {
                Name = "Suren",
                Gender = "male",
                Status = "active"
            };

            var users = new List<MyUser>
    {
        new MyUser { Id = 1, Name = "Suren one", Email = "Surenone@example.com", Gender = "male", Status = "active" },
        new MyUser { Id = 2, Name = "Suren two", Email = "Surentwo@example.com", Gender = "female", Status = "active" },
        new MyUser { Id = 3, Name = "Suren three", Email = "Surenthree@example.com", Gender = "male", Status = "inactive" }
    };

            // Mock the HTTP response
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(users))
                });

            // Act
            await userService.SearchAndExportToCSVFile(filePath, searchUser.Name, null, null, searchUser.Gender, searchUser.Status);

            // Assert
            httpMessageHandlerMock.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString() == $"https://gorest.co.in/public/v2/users?name={searchUser.Name}&gender={searchUser.Gender}&status={searchUser.Status}"
                ),
                ItExpr.IsAny<CancellationToken>());

            // Verify that the file was created and contains the expected CSV data
            Assert.That(File.Exists(filePath), Is.True);

            var csvData = await File.ReadAllTextAsync(filePath);

            Assert.That(csvData, Does.Contain("Id,Name,Email,Gender,Status"));
            Assert.That(csvData, Does.Contain("1,Suren one,Surenone@example.com,male,active"));
            Assert.That(csvData, Does.Contain("2,Suren two,Surentwo@example.com,female,active"));
            Assert.That(csvData, Does.Contain("3,Suren three,Surenthree@example.com,male,inactive"));

            // Cleanup
            File.Delete(filePath);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }
    }
}