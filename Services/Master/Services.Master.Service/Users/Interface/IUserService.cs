using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Service.Users.Interface
{
    public interface IUserService
    {
        Task<User> GetInfo(int userId);             
    }
}
