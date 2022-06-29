﻿using Services.Organization.Model.Employee;
using System.Threading.Tasks;

namespace Services.Organization.Service.Employee
{
    public interface IUserDataService
    {
        Task<UserDataModel> GetUserData(string key);
        Task<bool> UpdateUserData(string key, string data);
    }
}
