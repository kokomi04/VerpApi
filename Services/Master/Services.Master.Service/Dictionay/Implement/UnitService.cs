using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Dictionary;
using System.Linq;
using VErp.Services.Master.Service.Activity;
using VErp.Commons.Enums.MasterEnum;
using Newtonsoft.Json;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.Library;
using System.Linq.Expressions;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Master.Service.Dictionay.Implement
{
    public class UnitService : IUnitService
    {
        private readonly MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentContextService _currentContextService;

        public UnitService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<UnitService> logger
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService
            )
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _currentContextService = currentContextService;
        }

        public async Task<int> AddUnit(UnitInput data)
        {
            var validate = ValidateUnitInput(data);
            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            var info = await _masterContext.Unit.FirstOrDefaultAsync(u => u.UnitName == data.UnitName);
            if (info != null)
            {
                throw new BadRequestException(UnitErrorCode.UnitNameAlreadyExisted);
            }

            var unit = new Unit()
            {
                UnitName = data.UnitName,
                UnitStatusId = (int)data.UnitStatusId,
                DecimalPlace = data.DecimalPlace,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UpdatedDatetimeUtc = DateTime.UtcNow,
                IsDeleted = false,
            };

            await _masterContext.Unit.AddAsync(unit);
            await _masterContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Unit, unit.UnitId, $"Thêm đơn vị tính {data.UnitName}", data.JsonSerialize());

            return unit.UnitId;
        }

        public async Task<PageData<UnitOutput>> GetList(string keyword, EnumUnitStatus? unitStatusId, int page, int size, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();

            var query = (
                 from u in _masterContext.Unit
                 select new UnitOutput()
                 {
                     UnitId = u.UnitId,
                     UnitName = u.UnitName,
                     UnitStatusId = (EnumUnitStatus)u.UnitStatusId,
                     DecimalPlace = u.DecimalPlace
                 }
             );

            query = query.InternalFilter(filters);
            if (unitStatusId.HasValue)
            {
                query = from u in query
                        where u.UnitStatusId == unitStatusId.Value
                        select u;
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from u in query
                        where u.UnitName.Contains(keyword)
                        select u;
            }

            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query).ToListAsync();

            var total = await query.CountAsync();

            return (lst, total);
        }

        public async Task<UnitOutput> GetUnitInfo(int unitId)
        {
            var roleInfo = await _masterContext.Unit.Select(u => new UnitOutput()
            {
                UnitId = u.UnitId,
                UnitName = u.UnitName,
                UnitStatusId = (EnumUnitStatus)u.UnitStatusId,
                DecimalPlace = u.DecimalPlace
            }).FirstOrDefaultAsync(u => u.UnitId == unitId);

            if (roleInfo == null)
            {
                throw new BadRequestException(UnitErrorCode.UnitNotFound);
            }

            return roleInfo;
        }

        public async Task<bool> UpdateUnit(int unitId, UnitInput data)
        {
            var validate = ValidateUnitInput(data);
            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            var unitInfo = await _masterContext.Unit.FirstOrDefaultAsync(r => r.UnitId == unitId);
            if (unitInfo == null)
            {
                throw new BadRequestException(UnitErrorCode.UnitNotFound);
            }

            unitInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
            unitInfo.UnitName = data.UnitName;
            unitInfo.UnitStatusId = (int)data.UnitStatusId;
            unitInfo.DecimalPlace = data.DecimalPlace;
            await _masterContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Unit, unitId, $"Sửa đơn vị tính {data.UnitName}", data.JsonSerialize());
            return true;
        }

        public async Task<bool> DeleteUnit(int unitId)
        {
            var unitInfo = await _masterContext.Unit.FirstOrDefaultAsync(r => r.UnitId == unitId);
            if (unitInfo == null)
            {
                throw new BadRequestException(UnitErrorCode.UnitNotFound);
            }
            unitInfo.IsDeleted = true;
            await _masterContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.Unit, unitId, $"Xóa đơn vị tính {unitInfo.UnitName}", unitInfo.JsonSerialize());

            return true;
        }


        public async Task<IList<UnitOutput>> GetListByIds(IList<int> unitIds)
        {
            if (unitIds == null || unitIds.Count == 0)
            {
                return new List<UnitOutput>();
            }
            return await _masterContext.Unit
                .Where(u => unitIds.Contains(u.UnitId))
                .Select(u => new UnitOutput()
                {
                    UnitId = u.UnitId,
                    UnitName = u.UnitName,
                    UnitStatusId = (EnumUnitStatus)u.UnitStatusId,
                    DecimalPlace = u.DecimalPlace
                }).ToListAsync();
        }

        private Enum ValidateUnitInput(UnitInput unit)
        {
            unit.UnitName = (unit.UnitName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(unit.UnitName))
            {
                return UnitErrorCode.EmptyUnitName;
            }

            return GeneralCode.Success;
        }


    }
}
