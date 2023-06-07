using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UPS.Models;

namespace UPS.Services
{
    public interface IMyUserService
    {
        Task<MyUserResponse> GetUsersByUrl(string url);
        Task<ServerResponse> AddUser(MyUser newUser);
        Task<ServerResponse> UpdateUser(MyUser user);
        Task<ServerResponse> DeleteUser(int id);
        Task SearchAndExportToCSVFile(string filePath, string name, int? id, string email, string gender, string status);
    }
}
