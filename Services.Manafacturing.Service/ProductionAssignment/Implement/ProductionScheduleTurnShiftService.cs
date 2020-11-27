using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionAssignment;

namespace VErp.Services.Manafacturing.Service.ProductionAssignment.Implement
{
    public class ProductionScheduleTurnShiftService : IProductionScheduleTurnShiftService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ProductionScheduleTurnShiftService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionScheduleTurnShiftService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<long> CreateShift(int departmentId, long scheduleTurnId, long productionStepId, ProductionScheduleTurnShiftModel model)
        {
            var userModels = model.Users?.Where(u => u.Key > 0)?.ToList();
            if (userModels == null || userModels.Count() == 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Vui lòng nhập ít nhất một người dùng");
            }

            var assignmentInfo = _manufacturingDBContext.ProductionAssignment.FirstOrDefault(a => a.DepartmentId == departmentId && a.ProductionStepId == productionStepId && a.ScheduleTurnId == scheduleTurnId);
            if (assignmentInfo == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy phân công của công đoạn cho bộ phận này!");
            }

            var shift = _mapper.Map<ProductionScheduleTurnShift>(model);
            shift.DepartmentId = departmentId;
            shift.ScheduleTurnId = scheduleTurnId;
            shift.ProductionStepId = productionStepId;

            using (var trans = await _manufacturingDBContext.Database.BeginTransactionAsync())
            {
                await _manufacturingDBContext.ProductionScheduleTurnShift.AddAsync(shift);
                await _manufacturingDBContext.SaveChangesAsync();

                var users = new List<ProductionScheduleTurnShiftUser>();
                foreach (var u in userModels)
                {
                    var userData = _mapper.Map<ProductionScheduleTurnShiftUser>(u.Value);
                    userData.UserId = u.Key;
                    userData.ProductionScheduleTurnShiftId = shift.ProductionScheduleTurnShiftId;
                    users.Add(userData);
                }

                await _manufacturingDBContext.ProductionScheduleTurnShiftUser.AddRangeAsync(users);
                await _manufacturingDBContext.SaveChangesAsync();
                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.ProductionScheduleTurnShift, shift.ProductionScheduleTurnShiftId, $"Khai báo nhân công và chi phí {shift?.FromDate?.ToString("dd/MM/yyyy")}",
                     new
                     {
                         departmentId,
                         scheduleTurnId,
                         productionStepId,
                         model
                     }.JsonSerialize());
                return shift.ProductionScheduleTurnShiftId;
            }
        }

        public async Task<IList<ProductionScheduleTurnShiftModel>> GetShifts(int departmentId, long scheduleTurnId, long productionStepId)
        {
            var shifts = await _manufacturingDBContext.ProductionScheduleTurnShift.Include(s => s.ProductionScheduleTurnShiftUser).Where(a => a.DepartmentId == departmentId && a.ProductionStepId == productionStepId && a.ScheduleTurnId == scheduleTurnId)
                  .ToListAsync();

            return shifts.Select(s =>
            {
                var shift = _mapper.Map<ProductionScheduleTurnShiftModel>(s);
                var users = new Dictionary<int, ProductionScheduleTurnShiftUserModel>();
                foreach (var u in s.ProductionScheduleTurnShiftUser)
                {
                    users.Add(u.UserId, _mapper.Map<ProductionScheduleTurnShiftUserModel>(u));
                }
                shift.Users = users;
                return shift;
            }).ToList();
        }

        public async Task<bool> UpdateShift(int departmentId, long scheduleTurnId, long productionStepId, long productionScheduleTurnShiftId, ProductionScheduleTurnShiftModel model)
        {
            var userModels = model.Users?.Where(u => u.Key > 0)?.ToList();
            if (userModels == null || userModels.Count() == 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Vui lòng nhập ít nhất một người dùng");
            }

            var shiftInfo = await _manufacturingDBContext.ProductionScheduleTurnShift.FirstOrDefaultAsync(s => s.ProductionScheduleTurnShiftId == productionScheduleTurnShiftId);
            if (shiftInfo == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy ca trong hệ thống");
            }

            if (shiftInfo.DepartmentId != departmentId || shiftInfo.ScheduleTurnId != scheduleTurnId || shiftInfo.ProductionStepId != productionStepId)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            using (var trans = await _manufacturingDBContext.Database.BeginTransactionAsync())
            {
                _mapper.Map(model, shiftInfo);

                var oldUsers = await _manufacturingDBContext.ProductionScheduleTurnShiftUser.Where(u => u.ProductionScheduleTurnShiftId == productionScheduleTurnShiftId).ToListAsync();
                foreach (var u in oldUsers)
                {
                    u.IsDeleted = true;
                }

                var users = new List<ProductionScheduleTurnShiftUser>();
                foreach (var u in userModels)
                {
                    var userData = _mapper.Map<ProductionScheduleTurnShiftUser>(u.Value);
                    userData.UserId = u.Key;
                    userData.ProductionScheduleTurnShiftId = productionScheduleTurnShiftId;
                    users.Add(userData);
                }

                await _manufacturingDBContext.ProductionScheduleTurnShiftUser.AddRangeAsync(users);
                await _manufacturingDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.ProductionScheduleTurnShift, productionScheduleTurnShiftId, $"Cập nhật khai báo nhân công và chi phí {shiftInfo?.FromDate?.ToString("dd/MM/yyyy")}",
                     new
                     {
                         productionScheduleTurnShiftId,
                         model
                     }.JsonSerialize());
                return true;
            }
        }


        public async Task<bool> DeleteShift(int departmentId, long scheduleTurnId, long productionStepId, long productionScheduleTurnShiftId)
        {

            var shiftInfo = await _manufacturingDBContext.ProductionScheduleTurnShift.FirstOrDefaultAsync(s => s.ProductionScheduleTurnShiftId == productionScheduleTurnShiftId);
            if (shiftInfo == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy ca trong hệ thống");
            }


            if (shiftInfo.DepartmentId != departmentId || shiftInfo.ScheduleTurnId != scheduleTurnId || shiftInfo.ProductionStepId != productionStepId)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            using (var trans = await _manufacturingDBContext.Database.BeginTransactionAsync())
            {
                shiftInfo.IsDeleted = true;

                var oldUsers = await _manufacturingDBContext.ProductionScheduleTurnShiftUser.Where(u => u.ProductionScheduleTurnShiftId == productionScheduleTurnShiftId).ToListAsync();
                foreach (var u in oldUsers)
                {
                    u.IsDeleted = true;
                }


                await _manufacturingDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.ProductionScheduleTurnShift, productionScheduleTurnShiftId, $"Xóa khai báo nhân công và chi phí {shiftInfo?.FromDate?.ToString("dd/MM/yyyy")}",
                     new
                     {
                         productionScheduleTurnShiftId,
                     }.JsonSerialize());
                return true;
            }
        }
    }
}
