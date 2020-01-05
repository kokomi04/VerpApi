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

namespace VErp.Services.Master.Service.Dictionay.Implement
{
    public class UnitService : IUnitService
    {
        private readonly MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityService _activityService;

        public UnitService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<UnitService> logger
            , IActivityService activityService
            )
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityService = activityService;
        }

        public async Task<ServiceResult<int>> AddUnit(UnitInput data)
        {
            var validate = ValidateUnitInput(data);
            if (!validate.IsSuccess())
            {
                return validate;
            }

            var info = await _masterContext.Unit.FirstOrDefaultAsync(u => u.UnitName == data.UnitName);
            if (info != null)
            {
                return UnitErrorCode.UnitNameAlreadyExisted;
            }

            var unit = new Unit()
            {
                UnitName = data.UnitName,
                UnitStatusId = (int)data.UnitStatusId,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UpdatedDatetimeUtc = DateTime.UtcNow,
                IsDeleted = false,
            };

            await _masterContext.Unit.AddAsync(unit);
            await _masterContext.SaveChangesAsync();

            _activityService.CreateActivityAsync(EnumObjectType.Unit, unit.UnitId, $"Thêm đơn vị tính {data.UnitName}", null, data);

            return unit.UnitId;
        }



        public async Task<PageData<UnitOutput>> GetList(string keyword, EnumUnitStatus? unitStatusId, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = (
                 from u in _masterContext.Unit
                 select new UnitOutput()
                 {
                     UnitId = u.UnitId,
                     UnitName = u.UnitName,
                     UnitStatusId = (EnumUnitStatus)u.UnitStatusId,
                 }
             );

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

        public async Task<ServiceResult<UnitOutput>> GetUnitInfo(int unitId)
        {
            var roleInfo = await _masterContext.Unit.Select(u => new UnitOutput()
            {
                UnitId = u.UnitId,
                UnitName = u.UnitName,
                UnitStatusId = (EnumUnitStatus)u.UnitStatusId
            }).FirstOrDefaultAsync(u => u.UnitId == unitId);

            if (roleInfo == null)
            {
                return UnitErrorCode.UnitNotFound;
            }

            return roleInfo;
        }

        public async Task<Enum> UpdateUnit(int unitId, UnitInput data)
        {
            var validate = ValidateUnitInput(data);
            if (!validate.IsSuccess())
            {
                return validate;
            }

            var unitInfo = await _masterContext.Unit.FirstOrDefaultAsync(r => r.UnitId == unitId);
            if (unitInfo == null)
            {
                return UnitErrorCode.UnitNotFound;
            }

            var beforeChange = JsonConvert.SerializeObject(unitInfo);

            unitInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
            unitInfo.UnitName = data.UnitName;
            unitInfo.UnitStatusId = (int)data.UnitStatusId;
            await _masterContext.SaveChangesAsync();

            _activityService.CreateActivityAsync(EnumObjectType.Unit, unitId, $"Sửa đơn vị tính {data.UnitName}", beforeChange, data);
            return GeneralCode.Success;
        }

        public async Task<Enum> DeleteUnit(int unitId)
        {
            var unitInfo = await _masterContext.Unit.FirstOrDefaultAsync(r => r.UnitId == unitId);
            if (unitInfo == null)
            {
                return UnitErrorCode.UnitNotFound;
            }
            var beforeChange = JsonConvert.SerializeObject(unitInfo);
            unitInfo.IsDeleted = true;
            await _masterContext.SaveChangesAsync();
            _activityService.CreateActivityAsync(EnumObjectType.Unit, unitId, $"Xóa đơn vị tính {unitInfo.UnitName}", beforeChange, null);
            return GeneralCode.Success;
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
                    UnitStatusId = (EnumUnitStatus)u.UnitStatusId
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
