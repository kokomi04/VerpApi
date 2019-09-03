using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Services.Master.Service.Users.Interface;

namespace VErp.Services.Master.Service.Users.Implement
{
    public class UserService : IUserService
    {
        private MasterDBContext _masterContext;
        public UserService(MasterDBContext masterContext)
        {
            _masterContext = masterContext;
        }
        public async Task<User> GetInfo(int userId)
        {
            return await _masterContext.User.FirstOrDefaultAsync();
        }
    }
}
