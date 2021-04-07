using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Organization.Model.Deparment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes.Organization;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Infrastructure.EF.EFExtensions;

namespace Services.Organization.Service.Department.Implement
{
    public class SubsidiaryService : ISubsidiaryService
    {
        private readonly UnAuthorizeOrganizationContext _unAuthorizeOrganizationContext;

        private readonly IActivityLogService _activityLogService;
        private readonly ILogger<SubsidiaryService> _logger;
        private readonly ICurrentContextService _currentContext;

        private readonly IMapper _mapper;

        public SubsidiaryService(UnAuthorizeOrganizationContext unAuthorizeOrganizationContext
            , IActivityLogService activityLogService
            , ILogger<SubsidiaryService> logger
            , ICurrentContextService currentContext
            , IMapper mapper
            )
        {

            _unAuthorizeOrganizationContext = unAuthorizeOrganizationContext;
            _activityLogService = activityLogService;
            _logger = logger;
            _currentContext = currentContext;
            _mapper = mapper;
        }

        public async Task<PageData<SubsidiaryOutput>> GetList(string keyword, int page, int size, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();

            var query = _unAuthorizeOrganizationContext.Subsidiary.ProjectTo<SubsidiaryOutput>(_mapper.ConfigurationProvider);
            query = query.InternalFilter(filters, _currentContext.TimeZoneOffset);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(d => d.SubsidiaryCode.Contains(keyword) || d.SubsidiaryName.Contains(keyword));
            }

            var a = query.ToList();
            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query).ToListAsync();

            var subsidiaryIds = lst.Select(s => s.SubsidiaryId).ToList();

            var owners = await _unAuthorizeOrganizationContext.Employee.IgnoreQueryFilters()
                .Where(e => !e.IsDeleted && subsidiaryIds.Contains(e.SubsidiaryId) && e.EmployeeTypeId == (int)EnumEmployeeType.Owner)
                .ToListAsync();

            foreach (var item in lst)
            {
                var owner = owners.FirstOrDefault(o => o.SubsidiaryId == item.SubsidiaryId);
                if (owner != null)
                {
                    item.Owner = _mapper.Map<SubsidiaryOwnerModel>(owner);
                }
            }

            var total = await query.CountAsync();

            return (lst, total);
        }

        public async Task<int> Create(SubsidiaryModel data)
        {
            var info = await _unAuthorizeOrganizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryCode == data.SubsidiaryCode || d.SubsidiaryName == data.SubsidiaryName);

            if (info != null)
            {
                if (string.Compare(info.SubsidiaryCode, data.SubsidiaryCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw new BadRequestException(SubsidiaryErrorCode.SubsidiaryCodeExisted);
                }

                throw new BadRequestException(SubsidiaryErrorCode.SubsidiaryNameExisted);
            }
            if (data.ParentSubsidiaryId.HasValue)
            {
                var parent = await _unAuthorizeOrganizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryId == data.ParentSubsidiaryId.Value);
                if (parent == null)
                {
                    throw new BadRequestException(SubsidiaryErrorCode.SubsidiaryNotfound);
                }
            }

            info = _mapper.Map<Subsidiary>(data);

            await _unAuthorizeOrganizationContext.Subsidiary.AddAsync(info);
            await _unAuthorizeOrganizationContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Subsidiary, info.SubsidiaryId, $"Thêm cty con/chi nhánh {info.SubsidiaryCode}", data.JsonSerialize());

            return info.SubsidiaryId;
        }


        public async Task<bool> Update(int subsidiaryId, SubsidiaryModel data)
        {
            var info = await _unAuthorizeOrganizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryId != subsidiaryId && (d.SubsidiaryCode == data.SubsidiaryCode || d.SubsidiaryName == data.SubsidiaryName));

            if (info != null)
            {
                if (string.Compare(info.SubsidiaryCode, data.SubsidiaryCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw new BadRequestException(SubsidiaryErrorCode.SubsidiaryCodeExisted);
                }

                throw new BadRequestException(SubsidiaryErrorCode.SubsidiaryNameExisted);
            }

            info = await _unAuthorizeOrganizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryId == subsidiaryId);

            if (info == null)
            {
                throw new BadRequestException(SubsidiaryErrorCode.SubsidiaryNotfound);
            }

            if (data.ParentSubsidiaryId.HasValue)
            {
                var parent = await _unAuthorizeOrganizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryId == data.ParentSubsidiaryId.Value);
                if (parent == null)
                {
                    throw new BadRequestException(SubsidiaryErrorCode.SubsidiaryNotfound);
                }
            }

            _mapper.Map(data, info);
            //info.ParentSubsidiaryId = data.ParentSubsidiaryId;
            //info.SubsidiaryCode = data.SubsidiaryCode;
            //info.SubsidiaryName = data.SubsidiaryName;
            //info.Address = data.Address;
            //info.TaxIdNo = data.TaxIdNo;
            //info.PhoneNumber = data.PhoneNumber;
            //info.Email = data.Email;
            //info.Fax = data.Fax;
            //info.Description = data.Description;
            //info.UpdatedByUserId = _currentContext.UserId;
            //info.UpdatedDatetimeUtc = DateTime.UtcNow;

            await _unAuthorizeOrganizationContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Subsidiary, info.SubsidiaryId, $"Cập nhật cty con/chi nhánh {info.SubsidiaryCode}", data.JsonSerialize());

            return true;
        }

        public async Task<SubsidiaryOutput> GetInfo(int subsidiaryId)
        {
            var info = await _unAuthorizeOrganizationContext.Subsidiary.Where(d => d.SubsidiaryId == subsidiaryId).ProjectTo<SubsidiaryOutput>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();

            if (info == null)
            {
                throw new BadRequestException(SubsidiaryErrorCode.SubsidiaryNotfound);
            }

            var owner = await _unAuthorizeOrganizationContext.Employee.IgnoreQueryFilters()
                .Where(e => !e.IsDeleted && e.SubsidiaryId == subsidiaryId && e.EmployeeTypeId == (int)EnumEmployeeType.Owner)
                .FirstOrDefaultAsync();

            info.Owner = _mapper.Map<SubsidiaryOwnerModel>(owner);
            return info;
        }


        public async Task<bool> Delete(int subsidiaryId)
        {
            var info = await _unAuthorizeOrganizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryId == subsidiaryId);

            if (info == null)
            {
                throw new BadRequestException(SubsidiaryErrorCode.SubsidiaryNotfound);
            }

            info.IsDeleted = true;
            //info.UpdatedByUserId = _currentContext.UserId;
            //info.DeletedDatetimeUtc = DateTime.UtcNow;

            await _unAuthorizeOrganizationContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Subsidiary, info.SubsidiaryId, $"Xóa cty con/chi nhánh {info.SubsidiaryCode}", new { subsidiaryId }.JsonSerialize());

            return true;
        }

        public async Task<IList<SubsidiaryOutput>> GetList()
        {
            return await _unAuthorizeOrganizationContext.Subsidiary.ProjectTo<SubsidiaryOutput>(_mapper.ConfigurationProvider).ToListAsync();
        }
    }
}
