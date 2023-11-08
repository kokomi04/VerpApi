using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OpenXmlPowerTools;
using Services.Organization.Model.TimeKeeping;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IMapper _mapper;

        public OvertimePlanService(OrganizationDBContext organizationDBContext, IMapper mapper)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
        }

        public async Task<bool> AddOvertimePlan(OvertimePlanRequestModel model)
        {
            await DeleteOvertimePlan(model);

            var ePlan = _mapper.Map<List<OvertimePlan>>(model.OvertimePlans);

            await _organizationDBContext.OvertimePlan.AddRangeAsync(ePlan);

            await _organizationDBContext.SaveChangesAsync();

            return true;
        }


        public async Task<bool> DeleteOvertimePlan(OvertimePlanRequestModel model)
        {
            var overtimePlans = await _organizationDBContext.OvertimePlan.Where(p => p.AssignedDate >= model.FromDate.UnixToDateTime() && p.AssignedDate <= model.ToDate.UnixToDateTime()).ToListAsync();

            var mPlanSet = new HashSet<(long EmployeeId, long AssignedDate)>(model.OvertimePlans.Select(p => (p.EmployeeId, p.AssignedDate)));

            var ePlanToRemove = overtimePlans.Where(p => mPlanSet.Contains((p.EmployeeId, p.AssignedDate.GetUnix()))).ToList();

            if (overtimePlans == null || !overtimePlans.Any())
            {
                throw new BadRequestException("Không tìm thấy bản ghi nào để xóa");
            }

            _organizationDBContext.OvertimePlan.RemoveRange(ePlanToRemove);
            await _organizationDBContext.SaveChangesAsync();

            return true;
        }
        public async Task<IList<OvertimePlanModel>> GetListOvertimePlan(OvertimePlanRequestModel model)
        {
            return await _organizationDBContext.OvertimePlan
                .Where(p => p.AssignedDate >= model.FromDate.UnixToDateTime() && p.AssignedDate <= model.ToDate.UnixToDateTime())
                .ProjectTo<OvertimePlanModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync();
        }

    }
}