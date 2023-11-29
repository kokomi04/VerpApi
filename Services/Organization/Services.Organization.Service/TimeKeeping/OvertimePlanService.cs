using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OpenXmlPowerTools;
using Services.Organization.Model.TimeKeeping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Service.TimeKeeping
{
    public interface IOvertimePlanService
    {
        Task<bool> AddOvertimePlan(OvertimePlanRequestModel model);
        Task<bool> DeleteOvertimePlan(OvertimePlanRequestModel model);
        Task<IList<OvertimePlanModel>> GetListOvertimePlan(OvertimePlanRequestModel model);
    }

    public class OvertimePlanService : IOvertimePlanService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IShiftScheduleService _shiftScheduleService;
        private readonly IMapper _mapper;

        public OvertimePlanService(OrganizationDBContext organizationDBContext, IMapper mapper, IShiftScheduleService shiftScheduleService)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
            _shiftScheduleService = shiftScheduleService;
        }

        public async Task<bool> AddOvertimePlan(OvertimePlanRequestModel model)
        {
            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();

            try
            {
                var ePlanToRemove = _organizationDBContext.OvertimePlan.Where(p => p.AssignedDate >= model.FromDate.UnixToDateTime() && p.AssignedDate <= model.ToDate.UnixToDateTime());
                _organizationDBContext.OvertimePlan.RemoveRange(ePlanToRemove);

                var ePlan = _mapper.Map<List<OvertimePlan>>(model.OvertimePlans);

                await _organizationDBContext.OvertimePlan.AddRangeAsync(ePlan);

                await _organizationDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                throw new BadRequestException(GeneralCode.InternalError, ex.Message);
            }
        }


        public async Task<bool> DeleteOvertimePlan(OvertimePlanRequestModel model)
        {
            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();

            try
            {
                var overtimePlans = await _organizationDBContext.OvertimePlan.Where(p => p.AssignedDate >= model.FromDate.UnixToDateTime() && p.AssignedDate <= model.ToDate.UnixToDateTime()).ToListAsync();

                var mPlanSet = new HashSet<(long EmployeeId, long AssignedDate)>(model.OvertimePlans.Select(p => (p.EmployeeId, p.AssignedDate)));

                var ePlanToRemove = overtimePlans.Where(p => mPlanSet.Contains((p.EmployeeId, p.AssignedDate.GetUnix()))).ToList();

                if (ePlanToRemove.Any())
                {
                    _organizationDBContext.OvertimePlan.RemoveRange(ePlanToRemove);
                    await _organizationDBContext.SaveChangesAsync();
                }

                await trans.CommitAsync();

                return true;

            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                throw new BadRequestException(GeneralCode.InternalError, ex.Message);
            }
        }
        public async Task<IList<OvertimePlanModel>> GetListOvertimePlan(OvertimePlanRequestModel model)
        {
            var entity = _organizationDBContext.OvertimePlan
                .Where(p => p.AssignedDate >= model.FromDate.UnixToDateTime() && p.AssignedDate <= model.ToDate.UnixToDateTime()).AsNoTracking();

            if (model.DepartmentIds.Any())
            {
                var lstEmployees = await _shiftScheduleService.GetEmployeesByDepartments(model.DepartmentIds);
                entity = entity.Where(p => lstEmployees.Select(e => e[EmployeeConstants.EMPLOYEE_ID]).ToList().Contains(p.EmployeeId));
            }
            if (model.EmployeeIds.Any())
            {
                entity = entity.Where(p => model.EmployeeIds.Contains(p.EmployeeId));
            }

            return await entity.ProjectTo<OvertimePlanModel>(_mapper.ConfigurationProvider).ToArrayAsync();
        }

    }
}