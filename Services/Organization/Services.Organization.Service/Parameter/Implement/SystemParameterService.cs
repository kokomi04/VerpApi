using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Math.EC.Rfc7748;
using Services.Organization.Model.SystemParameter;
using System;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace Services.Organization.Service.Parameter.Implement
{
    public class SystemParameterService : ISystemParameterService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ILogger<SystemParameterService> _logger;
        private readonly ICurrentContextService _currentContextService;
        private readonly IActivityLogService _activityLogService;

        public SystemParameterService(
            OrganizationDBContext organizationDBContext, 
            ILogger<SystemParameterService> logger,
            ICurrentContextService currentContextService,
            IActivityLogService activityLogService)
        {
            _organizationDBContext = organizationDBContext;
            _logger = logger;
            _currentContextService = currentContextService;
            _activityLogService = activityLogService;
        }
        public async Task<int> CreateSystemParameter(SystemParameterModel spm)
        {
            var checkExistsFieldName = await _organizationDBContext.SystemParameter.Where(q => !q.IsDeleted).AnyAsync(x => x.FieldName.Equals(spm.Fieldname.Trim()));
            if (checkExistsFieldName)
            {
                throw new BadRequestException(SystemParameterErrorCode.SystemParameterAlreadyExisted);
            }
            using (var trans = await _organizationDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var sParameterInfo = new SystemParameter
                    {
                        FieldName = spm.Fieldname,
                        Name = spm.Name,
                        DataTypeId = (int)spm.DateTypeId,
                        Value = spm.Value,
                        UpdatedByUserId = _currentContextService.UserId,
                        UpdatedDateTimeUtc = DateTime.UtcNow,
                        CreatedByUserId = _currentContextService.UserId,
                        CreatedDateTimeUtc = DateTime.UtcNow,
                        IsDeleted = false,

                    };

                    await _organizationDBContext.AddAsync(sParameterInfo);
                    await _organizationDBContext.SaveChangesAsync();
                    trans.Commit();

                    await _activityLogService.CreateLog(EnumObjectType.SystemParameter, sParameterInfo.SystemParameterId, $"Thêm mới thông số hệ thống {spm.Fieldname}", spm.JsonSerialize());

                    return sParameterInfo.SystemParameterId;
                }
                catch (Exception)
                {
                    trans.TryRollbackTransaction();
                    throw;
                }
            }
        }

        public async Task<bool> DeleteSystemParameter(int keyId)
        {
            var sParameterInfo = await _organizationDBContext.SystemParameter.Where(q => !q.IsDeleted).FirstOrDefaultAsync(x => x.SystemParameterId.Equals(keyId));
            if (sParameterInfo == null)
                throw new BadRequestException(SystemParameterErrorCode.SystemParameterNotFound);

            using (var trans = await _organizationDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    sParameterInfo.IsDeleted = true;
                    sParameterInfo.UpdatedDateTimeUtc = DateTime.UtcNow;
                    sParameterInfo.UpdatedByUserId = _currentContextService.UserId;

                    await _organizationDBContext.SaveChangesAsync();
                    trans.Commit();

                    await _activityLogService.CreateLog(EnumObjectType.Stock, keyId, $"Xóa thông số hệ thống {sParameterInfo.FieldName}", sParameterInfo.JsonSerialize());

                    return true;
                }
                catch (Exception)
                {
                    trans.TryRollbackTransaction();
                    throw;

                }
            }
        }

        public async Task<PageData<SystemParameterModel>> GetList(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            IQueryable<SystemParameter> query = _organizationDBContext.SystemParameter.Where(x=> !x.IsDeleted);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.FieldName.Contains(keyword)
                || x.Name.Contains(keyword));
            }

            var total = await query.CountAsync();
            var lstData = await query
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new SystemParameterModel
                {
                    SystemParameterId = x.SystemParameterId,
                    Fieldname = x.FieldName,
                    DateTypeId = (VErp.Commons.Enums.MasterEnum.EnumDataType)x.DataTypeId,
                    Name = x.Name,
                    Value = x.Value
                })
                .ToListAsync();

            return (lstData, total);

        }

        public async Task<SystemParameterModel> GetSystemParameterById(int keyId)
        {
            var stockInfo = await _organizationDBContext.SystemParameter.Where(q => !q.IsDeleted).FirstOrDefaultAsync(p => p.SystemParameterId == keyId);
            if (stockInfo == null)
            {
                throw new BadRequestException(SystemParameterErrorCode.SystemParameterNotFound);
            }
            return new SystemParameterModel()
            {
                SystemParameterId = stockInfo.SystemParameterId,
                Fieldname = stockInfo.FieldName,
                DateTypeId = (VErp.Commons.Enums.MasterEnum.EnumDataType)stockInfo.DataTypeId,
                Name = stockInfo.Name,
                Value = stockInfo.Value
            };
        }

        public async Task<bool> UpdateSystemParameter(int keyId, SystemParameterModel spm)
        {
            var checkExistsFieldName = await _organizationDBContext.SystemParameter.Where(q => !q.IsDeleted).AnyAsync(x => x.FieldName.Equals(spm.Fieldname.Trim()));
            if(checkExistsFieldName)
            {
                throw new BadRequestException(SystemParameterErrorCode.SystemParameterAlreadyExisted);
            }
            using(var trans = await _organizationDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var sParameterInfo = await _organizationDBContext.SystemParameter.Where(q => !q.IsDeleted).FirstOrDefaultAsync(x => x.SystemParameterId.Equals(keyId));
                    if(sParameterInfo == null)
                        throw new BadRequestException(SystemParameterErrorCode.SystemParameterNotFound);
                    sParameterInfo.FieldName = spm.Fieldname;
                    sParameterInfo.Name = spm.Name;
                    sParameterInfo.DataTypeId = (int)spm.DateTypeId;
                    sParameterInfo.Value = spm.Value;
                    sParameterInfo.UpdatedByUserId = _currentContextService.UserId;
                    sParameterInfo.UpdatedDateTimeUtc = DateTime.UtcNow;

                    await _organizationDBContext.SaveChangesAsync();
                    trans.Commit();

                    await _activityLogService.CreateLog(EnumObjectType.SystemParameter, keyId, $"Cập nhật thông số hệ thống {spm.Fieldname}", spm.JsonSerialize());

                    return true;
                }
                catch(Exception)
                {
                    trans.TryRollbackTransaction();
                    throw;
                }
            }
            
        }
    }
}
