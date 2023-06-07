using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UPS.Models;

namespace UPS.Services
{
    public class MyUserService : IMyUserService
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public MyUserService(HttpClient httpClient, ILogger logger)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<ServerResponse> AddUser(MyUser newUser)
        {   
            // Perform validation on the input fields
            if (string.IsNullOrWhiteSpace(newUser.Name))
            {
                throw new ArgumentException("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(newUser.Email) || !IsValidEmail(newUser.Email))
            {
                throw new ArgumentException("Email is required and should be a valid email address.");
            }

            string gender = newUser.Gender?.ToLower() ?? "";
            if (gender != "male" && gender != "female")
            {
                throw new ArgumentException("Gender should be either 'Male' or 'Female'.");
            }

            string status = newUser.Status?.ToLower() ?? "";
            if (status != "active" && status != "inactive")
            {
                throw new ArgumentException("Status should be either 'Active' or 'Inactive'.");
            }

            // Convert property values to lowercase
            newUser.Name = newUser.Name.ToLower();
            newUser.Email = newUser.Email.ToLower();
            newUser.Gender = gender;
            newUser.Status = status;

            var content = new StringContent(JsonSerializer.Serialize(newUser), Encoding.UTF8, "application/json");

            //retreive the users
            var response = await _httpClient.PostAsync("users", content);
            response.EnsureSuccessStatusCode();

            //store the response in return object
            var serverResponse = new ServerResponse
            {
                StatusCode = response.StatusCode
            };

            if (response.StatusCode == HttpStatusCode.Created)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<MyUser>(responseBody);

                if (result == null)
                {
                    Log.Error($"Failed to deserialize the API response while adding a user.");
                    throw new Exception(responseBody);
                }

                serverResponse.User = result;
            }

            return serverResponse;
        }

        public async Task<ServerResponse> DeleteUser(int id)
        {
            //delete the user
            var response = await _httpClient.DeleteAsync($"users/{id}");

            //store the response in return object
            var serverResponse = new ServerResponse
            {
                StatusCode = response.StatusCode
            };

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                serverResponse.User = null;
            }

            return serverResponse;
        }

        //get users by page vise
        public async Task<MyUserResponse> GetUsersByUrl(string url)
        {
            //retreive the users by page
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();


            var users = await JsonSerializer.DeserializeAsync<IEnumerable<MyUser>>(
                await response.Content.ReadAsStreamAsync());

            // Read the pagination headers from the response
            var nextPageUrl = response.Headers.Contains("x-links-next")
                ? response.Headers.GetValues("x-links-next").First()
                : null;
            var previousPageUrl = response.Headers.Contains("x-links-previous")
                ? response.Headers.GetValues("x-links-previous").First()
                : null;
            var currentPage = response.Headers.Contains("x-pagination-page")
                ? int.Parse(response.Headers.GetValues("x-pagination-page").First())
                : 0;
            var totalPages = response.Headers.Contains("x-pagination-pages")
                ? int.Parse(response.Headers.GetValues("x-pagination-pages").First())
                : 0;

            //if the page is the last page, disable the next button
            if(currentPage >= totalPages)
            {
                nextPageUrl = "";
            }

            return new MyUserResponse
            {
                Users = users,
                NextPageUrl = nextPageUrl,
                PreviousPageUrl = previousPageUrl,
                CurrentPage = currentPage,
                TotalPages = totalPages
            };
        }

        public async Task<IEnumerable<MyUser>> SearchUsers(string name = null, int? id = null, string email = null, string gender = null, string status = null)
        {
            var queryParams = new List<string>();

            //validation for the input data
            if (!string.IsNullOrWhiteSpace(name))
                queryParams.Add($"name={Uri.EscapeDataString(name)}");

            if (id.HasValue && id != 0)
                queryParams.Add($"id={id.Value}");

            if (!string.IsNullOrWhiteSpace(email))
                queryParams.Add($"email={Uri.EscapeDataString(email)}");

            if (!string.IsNullOrWhiteSpace(gender))
                queryParams.Add($"gender={Uri.EscapeDataString(gender)}");

            if (!string.IsNullOrWhiteSpace(status))
                queryParams.Add($"status={Uri.EscapeDataString(status)}");

            var queryString = string.Join("&", queryParams);

            var response = await _httpClient.GetAsync($"users?{queryString}");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<IEnumerable<MyUser>>(responseBody);
            if (result == null)
            {
                Log.Error($"Failed to deserialize the API response while retreiving searching users.");
                throw new Exception("Failed to deserialize the API response while retreiving searching users.");
            }
            return result;
        }

        //method to return the string in csv format
        public async Task SearchAndExportToCSVFile(string filePath, string name = null, int? id = null, string email = null, string gender = null, string status = null)
        {
            var users = await SearchUsers(name, id, email, gender, status);

            using (var writer = new StreamWriter(filePath))
            {
                await writer.WriteAsync($"Id,");
                await writer.WriteAsync($"Name,");
                await writer.WriteAsync($"Email,");
                await writer.WriteAsync($"Gender,");
                await writer.WriteAsync($"Status,");
                await writer.WriteLineAsync();

                foreach (var user in users)
                {
                    await writer.WriteAsync($"{user.Id},");
                    await writer.WriteAsync($"{user.Name},");
                    await writer.WriteAsync($"{user.Email},");
                    await writer.WriteAsync($"{user.Gender},");
                    await writer.WriteAsync($"{user.Status},");
                    await writer.WriteLineAsync();
                }
            }
        }


        public async Task<ServerResponse> UpdateUser(MyUser user)
        {            
            // Perform validation on the input fields
            if (string.IsNullOrWhiteSpace(user.Name))
            {
                throw new ArgumentException("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(user.Email) || !IsValidEmail(user.Email))
            {
                throw new ArgumentException("Email is required and should be a valid email address.");
            }

            string gender = user.Gender?.ToLower() ?? "";
            if (gender != "male" && gender != "female")
            {
                throw new ArgumentException("Gender should be either 'Male' or 'Female'.");
            }

            string status = user.Status?.ToLower() ?? "";
            if (status != "active" && status != "inactive")
            {
                throw new ArgumentException("Status should be either 'Active' or 'Inactive'.");
            }

            // Convert property values to lowercase
            user.Name = user.Name.ToLower();
            user.Email = user.Email.ToLower();
            user.Gender = gender;
            user.Status = status;


            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

            //call the api
            var response = await _httpClient.PutAsync($"users/{user.Id}", content);
            response.EnsureSuccessStatusCode();

            var serverResponse = new ServerResponse
            {
                StatusCode = response.StatusCode
            };

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<MyUser>(responseBody);

                if (result == null)
                {
                    Log.Error($"Failed to deserialize the API response while updating a user.");
                    throw new Exception(responseBody);
                }

                serverResponse.User = result;
            }

            return serverResponse;
        }

        //method to validate the email
        private bool IsValidEmail(string email)
        {
            try
            {
                var address = new System.Net.Mail.MailAddress(email);
                return address.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}
