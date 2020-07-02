using Microsoft.EntityFrameworkCore;
using Services.Organization.Model.Employee;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Service.Employee.Implement
{
    public class UserDataService : IUserDataService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICurrentContextService _currentContextService;
        public UserDataService(OrganizationDBContext organizationDBContext,
            ICurrentContextService currentContextService
            )
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
        }

        public async Task<UserDataModel> GetUserData(string key)
        {
            var info = await _organizationDBContext.UserData.FirstOrDefaultAsync(u => u.DataKey == key);
            return new UserDataModel()
            {
                DataContent = info?.DataContent
            };
        }

        public async Task<bool> UpdateUserData(string key, string data)
        {
            var info = await _organizationDBContext.UserData.FirstOrDefaultAsync(u => u.DataKey == key);
            if (info != null)
            {
                info.DataContent = data;
                info.UpdatedDatetimeUtc = DateTime.UtcNow;
            }
            else
            {
                await _organizationDBContext.UserData.AddAsync(new UserData()
                {
                    UserId = _currentContextService.UserId,
                    DataKey = key,
                    DataContent = data,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow
                });
            }

            await _organizationDBContext.SaveChangesAsync();

            return true;
        }
    }
}
