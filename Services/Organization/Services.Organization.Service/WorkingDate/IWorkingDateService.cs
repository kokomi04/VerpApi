using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Organization.Model.WorkingDate;

namespace VErp.Services.Organization.Service.WorkingDate
{
    public interface IWorkingDateService
    {
        Task<WorkingDateModel> Create(WorkingDateModel model);
        Task<bool> Update(WorkingDateModel req);
        Task<WorkingDateModel> GetWorkingDateByUserId(int userId);
    }
}
