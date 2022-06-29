using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Master.Unit;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Dictionary;

namespace VErp.Services.Master.Service.Dictionay.Implement
{
    public class UnitService : IUnitService
    {
        private readonly MasterDBContext _masterContext;
        private readonly ObjectActivityLogFacade _unitActivityLog;

        public UnitService(MasterDBContext masterContext
            , IActivityLogService activityLogService
            )
        {
            _masterContext = masterContext;
            _unitActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Unit);
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


            await _unitActivityLog.LogBuilder(() => UnitActivityLogMessage.Create)
               .MessageResourceFormatDatas(unit.UnitName)
               .ObjectId(unit.UnitId)
               .JsonData(data.JsonSerialize())
               .CreateLog();

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

            await _unitActivityLog.LogBuilder(() => UnitActivityLogMessage.Update)
            .MessageResourceFormatDatas(unitInfo.UnitName)
            .ObjectId(unitInfo.UnitId)
            .JsonData(data.JsonSerialize())
            .CreateLog();

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

            await _unitActivityLog.LogBuilder(() => UnitActivityLogMessage.Delete)
            .MessageResourceFormatDatas(unitInfo.UnitName)
            .ObjectId(unitInfo.UnitId)
            .JsonData(unitInfo.JsonSerialize())
            .CreateLog();

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
