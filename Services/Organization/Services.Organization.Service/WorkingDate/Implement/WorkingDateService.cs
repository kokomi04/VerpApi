using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.WorkingDate;
using VErp.Infrastructure.EF.EFExtensions;
using Org.BouncyCastle.Ocsp;
using Verp.Resources.Master.Config.DataConfig;
using WorkingDateBase = VErp.Infrastructure.EF.OrganizationDB.WorkingDate;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Organization.Service.WorkingDate.Implement
{
    public class WorkingDateService : IWorkingDateService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _dataConfigActivityLog;

        public WorkingDateService(OrganizationDBContext organizationDBContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
            _dataConfigActivityLog = activityLogService.CreateObjectTypeActivityLog(Commons.Enums.MasterEnum.EnumObjectType.WorkingDateConfig);
        }
        public async Task<WorkingDateModel> Create(WorkingDateModel model)
        {
            using (var trans = _organizationDBContext.Database.BeginTransaction())
            {
                try
                {
                    var workingDate = await _organizationDBContext.WorkingDate.FirstOrDefaultAsync(x => x.UserId == model.UserId);
                    if (workingDate != null)
                    {
                        throw new BadRequestException("User đã được tạo ngày làm việc!");
                    }
                    var info = _mapper.Map<WorkingDateBase>(model);
                    await _organizationDBContext.WorkingDate.AddAsync(info);
                    await _organizationDBContext.SaveChangesAsync();
                    trans.Commit();

                    return _mapper.Map<WorkingDateModel>(info);
                }
                catch (Exception)
                {
                    trans.TryRollbackTransaction();
                    throw;
                }
            }


        }

        public async Task<WorkingDateModel> GetWorkingDateByUserId(int userId)
        {
            var info = await _organizationDBContext.WorkingDate.FirstOrDefaultAsync(x => x.UserId == userId);
            if (info == null)
            {
                info = new WorkingDateBase()
                {
                    WorkingDateId = 0,
                    UserId = userId,
                    SubsidiaryId = 0
                };
            }
            return _mapper.Map<WorkingDateModel>(info);
        }

        public async Task<bool> Update(WorkingDateModel req)
        {
            using (var trans = _organizationDBContext.Database.BeginTransaction())
                try
                {
                    var info = await _organizationDBContext.WorkingDate.FirstOrDefaultAsync(x => x.UserId == req.UserId);
                    if (info == null)
                    {
                        info = _mapper.Map<WorkingDateBase>(req);
                        await _organizationDBContext.WorkingDate.AddAsync(info);
                    }
                    else
                    {
                        _mapper.Map(req, info);
                    }

                    await _organizationDBContext.SaveChangesAsync();
                    trans.Commit();
                    return true;


                }
                catch (Exception)
                {
                    trans.TryRollbackTransaction();
                    return false;
                    throw;
                }

        }
    }
}
