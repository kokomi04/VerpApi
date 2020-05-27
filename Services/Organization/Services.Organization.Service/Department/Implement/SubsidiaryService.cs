﻿using AutoMapper;
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

        private readonly IMapper _mapper;

        public SubsidiaryService(OrganizationDBContext organizationContext
            , IActivityLogService activityLogService
            , ILogger<SubsidiaryService> logger
            , ICurrentContextService currentContext
            , IMapper mapper
            )
        {

            _organizationContext = organizationContext;
            _activityLogService = activityLogService;
            _logger = logger;
            _currentContext = currentContext;
            _mapper = mapper;
        }

        public async Task<PageData<SubsidiaryOutput>> GetList(string keyword, int page, int size, Dictionary<string, List<string>> filters = null)
        {
            keyword = (keyword ?? "").Trim();

            var query = _organizationContext.Subsidiary.ProjectTo<SubsidiaryOutput>(_mapper.ConfigurationProvider);
            query = query.InternalFilter(filters);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(d => d.SubsidiaryCode.Contains(keyword) || d.SubsidiaryName.Contains(keyword));
            }

            var a = query.ToList();
            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query).ToListAsync();

            var total = await query.CountAsync();

            return (lst, total);
        }

        public async Task<int> Create(SubsidiaryModel data)
        {
            var info = await _organizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryCode == data.SubsidiaryCode || d.SubsidiaryName == data.SubsidiaryName);

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
                var parent = await _organizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryId == data.ParentSubsidiaryId.Value);
                if (parent == null)
                {
                    throw new BadRequestException(SubsidiaryErrorCode.SubsidiaryNotfound);
                }
            }

            info = _mapper.Map<Subsidiary>(data);

            await _organizationContext.Subsidiary.AddAsync(info);
            await _organizationContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Subsidiary, info.SubsidiaryId, $"Thêm cty con/chi nhánh {info.SubsidiaryCode}", data.JsonSerialize());
            return info.SubsidiaryId;
        }


        public async Task<bool> Update(int subsidiaryId, SubsidiaryModel data)
        {
            var info = await _organizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryId != subsidiaryId && (d.SubsidiaryCode == data.SubsidiaryCode || d.SubsidiaryName == data.SubsidiaryName));

            if (info != null)
            {
                if (string.Compare(info.SubsidiaryCode, data.SubsidiaryCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw new BadRequestException(SubsidiaryErrorCode.SubsidiaryCodeExisted);
                }

                throw new BadRequestException(SubsidiaryErrorCode.SubsidiaryNameExisted);
            }

            info = await _organizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryId == subsidiaryId);

            if (info == null)
            {
                throw new BadRequestException(SubsidiaryErrorCode.SubsidiaryNotfound);
            }

            if (data.ParentSubsidiaryId.HasValue)
            {
                var parent = await _organizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryId == data.ParentSubsidiaryId.Value);
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

            await _organizationContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Subsidiary, info.SubsidiaryId, $"Cập nhật cty con/chi nhánh {info.SubsidiaryCode}", data.JsonSerialize());

            return true;
        }

        public async Task<SubsidiaryModel> GetInfo(int subsidiaryId)
        {

            var info = await _organizationContext.Subsidiary.Where(d => d.SubsidiaryId == subsidiaryId).ProjectTo<SubsidiaryModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();

            if (info == null)
            {
                throw new BadRequestException(SubsidiaryErrorCode.SubsidiaryNotfound);
            }
            return info;
        }


        public async Task<bool> Delete(int subsidiaryId)
        {
            var info = await _organizationContext.Subsidiary.FirstOrDefaultAsync(d => d.SubsidiaryId == subsidiaryId);

            if (info == null)
            {
                throw new BadRequestException(SubsidiaryErrorCode.SubsidiaryNotfound);
            }

            info.IsDeleted = true;
            //info.UpdatedByUserId = _currentContext.UserId;
            //info.DeletedDatetimeUtc = DateTime.UtcNow;

            await _organizationContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Subsidiary, info.SubsidiaryId, $"Xóa cty con/chi nhánh {info.SubsidiaryCode}", new { subsidiaryId }.JsonSerialize());

            return true;
        }

    }
}
