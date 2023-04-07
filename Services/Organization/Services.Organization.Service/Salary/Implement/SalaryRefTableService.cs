using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary.Implement
{
    public class SalaryRefTableService : ISalaryRefTableService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _salaryRefTableActivityLog;

        public SalaryRefTableService(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, IMapper mapper, IActivityLogService activityLogService)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
            _mapper = mapper;
            _salaryRefTableActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryRefTable);
        }

        public async Task<int> Create(SalaryRefTableModel model)
        {
            var info = _mapper.Map<SalaryRefTable>(model);
            await _organizationDBContext.SalaryRefTable.AddAsync(info);
            await _organizationDBContext.SaveChangesAsync();
            await _salaryRefTableActivityLog.CreateLog(info.SalaryRefTableId, $"Thêm mới bảng liên kết {model.RefTableCode} vào bảng lương", model.JsonSerialize());
            return info.SalaryRefTableId;
        }

        public async Task<bool> Delete(int salaryRefTableId)
        {
            var info = await _organizationDBContext.SalaryRefTable.FirstOrDefaultAsync(s => s.SalaryRefTableId == salaryRefTableId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            _organizationDBContext.SalaryRefTable.Remove(info);
            await _organizationDBContext.SaveChangesAsync();
            await _salaryRefTableActivityLog.CreateLog(info.SalaryRefTableId, $"Xóa bảng liên kết {info.RefTableCode} khỏi bảng lương", info.JsonSerialize());
            return true;
        }

        public async Task<IList<SalaryRefTableModel>> GetList()
        {
            return await _organizationDBContext.SalaryRefTable.ProjectTo<SalaryRefTableModel>(_mapper.ConfigurationProvider)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
        }

        public async Task<bool> Update(int salaryRefTableId, SalaryRefTableModel model)
        {
            var info = await _organizationDBContext.SalaryRefTable.FirstOrDefaultAsync(s => s.SalaryRefTableId == salaryRefTableId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            _mapper.Map(model, info);
            info.SalaryRefTableId = salaryRefTableId;
            await _organizationDBContext.SaveChangesAsync();
            await _salaryRefTableActivityLog.CreateLog(info.SalaryRefTableId, $"Cập nhật bảng liên kết {model.RefTableCode} vào bảng lương", model.JsonSerialize());
            return true;
        }
    }
}
