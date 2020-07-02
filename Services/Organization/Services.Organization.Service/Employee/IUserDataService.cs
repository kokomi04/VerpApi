using Services.Organization.Model.Employee;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Services.Organization.Service.Employee
{
    public interface IUserDataService
    {
        Task<UserDataModel> GetUserData(string key);
        Task<bool> UpdateUserData(string key, string data);
    }
}
