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

namespace Services.Organization.Service.Department.Implement
{
    public class SubsidiaryService : ISubsidiaryService
    {
        private readonly OrganizationDBContext _organizationContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger<SubsidiaryService> _logger;
        private readonly ICurrentContextService _currentContext;

        private readonly IConfigurationProvider cfg = new MapperConfiguration(c =>
            {
                c.CreateMap<Subsidiary, SubsidiaryOutput>();
                c.CreateMap<Subsidiary, SubsidiaryModel>();
            });

        public SubsidiaryService(OrganizationDBContext organizationContext
            , IActivityLogService activityLogService
            , ILogger<SubsidiaryService> logger
            , ICurrentContextService currentContext
            )
        {

            _organizationContext = organizationContext;
            _activityLogService = activityLogService;
            _logger = logger;
            _currentContext = currentContext;
        }

        public async Task<PageData<SubsidiaryOutput>> GetList(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _organizationContext.Subsidiary.ProjectTo<SubsidiaryOutput>(cfg);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(d => d.SubsidiaryCode.Contains(keyword) || d.SubsidiaryName.Contains(keyword));
            }

            var a = query.ToList();
            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query).ToListAsync();

            var total = await query.CountAsync();

            return (lst, total);
        }

        public async Task<ServiceResult<int>> Create(SubsidiaryModel data)
        {
            var info = await _organizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryCode == data.SubsidiaryCode || d.SubsidiaryName == data.SubsidiaryName);

            if (info != null)
            {
                if (string.Compare(info.SubsidiaryCode, data.SubsidiaryCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return SubsidiaryErrorCode.SubsidiaryCodeExisted;
                }

                return SubsidiaryErrorCode.SubsidiaryNameExisted;
            }
            if (data.ParentSubsidiaryId.HasValue)
            {
                var parent = await _organizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryId == data.ParentSubsidiaryId.Value);
                if (parent == null)
                {
                    return SubsidiaryErrorCode.SubsidiaryNotfound;
                }
            }

            info = new Subsidiary()
            {
                ParentSubsidiaryId = data.ParentSubsidiaryId,
                SubsidiaryCode = data.SubsidiaryCode,
                SubsidiaryName = data.SubsidiaryName,
                Address = data.Address,
                TaxIdNo = data.TaxIdNo,
                PhoneNumber = data.PhoneNumber,
                Email = data.Email,
                Fax = data.Fax,
                Description = data.Description,
                IsDeleted = false,
                UpdatedByUserId = _currentContext.UserId,
                CreatedByUserId = _currentContext.UserId,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UpdatedDatetimeUtc = DateTime.UtcNow,
                DeletedDatetimeUtc = null

            };

            await _organizationContext.Subsidiary.AddAsync(info);
            await _organizationContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Subsidiary, info.SubsidiaryId, $"Thêm cty con/chi nhánh {info.SubsidiaryCode}", data.JsonSerialize());
            return info.SubsidiaryId;
        }


        public async Task<ServiceResult> Update(int subsidiaryId, SubsidiaryModel data)
        {
            var info = await _organizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryId != subsidiaryId && (d.SubsidiaryCode == data.SubsidiaryCode || d.SubsidiaryName == data.SubsidiaryName));

            if (info != null)
            {
                if (string.Compare(info.SubsidiaryCode, data.SubsidiaryCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return SubsidiaryErrorCode.SubsidiaryCodeExisted;
                }

                return SubsidiaryErrorCode.SubsidiaryNameExisted;
            }

            info = await _organizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryId == subsidiaryId);

            if (info == null)
            {
                return SubsidiaryErrorCode.SubsidiaryNotfound;
            }

            if (data.ParentSubsidiaryId.HasValue)
            {
                var parent = await _organizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryId == data.ParentSubsidiaryId.Value);
                if (parent == null)
                {
                    return SubsidiaryErrorCode.SubsidiaryNotfound;
                }
            }

            info.ParentSubsidiaryId = data.ParentSubsidiaryId;
            info.SubsidiaryCode = data.SubsidiaryCode;
            info.SubsidiaryName = data.SubsidiaryName;
            info.Address = data.Address;
            info.TaxIdNo = data.TaxIdNo;
            info.PhoneNumber = data.PhoneNumber;
            info.Email = data.Email;
            info.Fax = data.Fax;
            info.Description = data.Description;
            info.UpdatedByUserId = _currentContext.UserId;
            info.UpdatedDatetimeUtc = DateTime.UtcNow;

            await _organizationContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Subsidiary, info.SubsidiaryId, $"Cập nhật cty con/chi nhánh {info.SubsidiaryCode}", data.JsonSerialize());
            return GeneralCode.Success;
        }

        public async Task<ServiceResult<SubsidiaryModel>> GetInfo(int subsidiaryId)
        {

            var info = await _organizationContext.Subsidiary.Where(d => d.SubsidiaryId == subsidiaryId).ProjectTo<SubsidiaryModel>(cfg).FirstOrDefaultAsync();

            if (info == null)
            {
                return SubsidiaryErrorCode.SubsidiaryNotfound;
            }
            return info;
        }


        public async Task<ServiceResult> Delete(int subsidiaryId)
        {
            var info = await _organizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryId == subsidiaryId);

            if (info == null)
            {
                return SubsidiaryErrorCode.SubsidiaryNotfound;
            }

            info.IsDeleted = true;
            info.UpdatedByUserId = _currentContext.UserId;
            info.DeletedDatetimeUtc = DateTime.UtcNow;

            await _organizationContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Subsidiary, info.SubsidiaryId, $"Xóa cty con/chi nhánh {info.SubsidiaryCode}", new { subsidiaryId }.JsonSerialize());
            return GeneralCode.Success;
        }

    }
}
