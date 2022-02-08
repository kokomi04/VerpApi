using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Services.Organization.Model.TimeKeeping;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Service.TimeKeeping
{
    public interface ITimeSheetService
    {
        Task<long> AddTimeSheet(TimeSheetModel model);
        Task<bool> DeleteTimeSheet(long timeSheetId);
        Task<IList<TimeSheetModel>> GetListTimeSheet();
        Task<TimeSheetModel> GetTimeSheet(long timeSheetId);
        Task<bool> UpdateTimeSheet(long timeSheetId, TimeSheetModel model);


        Task<CategoryNameModel> GetFieldDataForMapping();

        Task<bool> ImportTimeSheetFromMapping(ImportExcelMapping mapping, Stream stream);
    }

    public class TimeSheetService : ITimeSheetService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;

        public TimeSheetService(OrganizationDBContext organizationDBContext, IMapper mapper)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
        }

        public async Task<long> AddTimeSheet(TimeSheetModel model)
        {
            if (model.TimeOut < model.TimeIn)
                throw new BadRequestException(GeneralCode.InvalidParams, "Thời gian checkin phải nhỏ hơn thời gian checkout");

            var entity = _mapper.Map<TimeSheet>(model);

            await _organizationDBContext.TimeSheet.AddAsync(entity);
            await _organizationDBContext.SaveChangesAsync();

            return entity.TimeSheetId;
        }

        public async Task<bool> UpdateTimeSheet(long timeSheetId, TimeSheetModel model)
        {
            var timeSheet = await _organizationDBContext.TimeSheet.FirstOrDefaultAsync(x => x.TimeSheetId == timeSheetId);
            if (timeSheet == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            model.TimeSheetId = timeSheetId;
            _mapper.Map(model, timeSheet);

            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteTimeSheet(long timeSheetId)
        {
            var timeSheet = await _organizationDBContext.TimeSheet.FirstOrDefaultAsync(x => x.TimeSheetId == timeSheetId);
            if (timeSheet == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            timeSheet.IsDeleted = true;
            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<TimeSheetModel> GetTimeSheet(long timeSheetId)
        {
            var timeSheet = await _organizationDBContext.TimeSheet.FirstOrDefaultAsync(x => x.TimeSheetId == timeSheetId);
            if (timeSheet == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            return _mapper.Map<TimeSheetModel>(timeSheet);
        }

        public async Task<IList<TimeSheetModel>> GetListTimeSheet()
        {
            var query = _organizationDBContext.TimeSheet.AsNoTracking();

            return await query.Select(x => new TimeSheetModel
            {
                Date = x.Date.GetUnix(),
                TimeIn = x.TimeIn.TotalSeconds,
                TimeOut = x.TimeOut.TotalSeconds,
                EmployeeId = x.EmployeeId,
                TimeSheetId = x.TimeSheetId
            }).ToListAsync();
        }


        public async Task<CategoryNameModel> GetFieldDataForMapping()
        {
            var result = new CategoryNameModel()
            {
                CategoryCode = "TimeSheet",
                CategoryTitle = "Chấm công",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            var fields = Utils.GetFieldNameModels<TimeSheetImportFieldModel>();
            result.Fields = fields;
            return result;
        }

        public async Task<bool> ImportTimeSheetFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            var employees = await _organizationDBContext.Employee.ToListAsync();

            var lstData = reader.ReadSheetEntity<TimeSheetImportFieldModel>(mapping, (entity, propertyName, value) =>
            {

                if (propertyName == nameof(TimeSheetImportFieldModel.EmployeeId))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        var val = value?.NormalizeAsInternalName();

                        var employee = employees
                        .Where(e => e.EmployeeCode.NormalizeAsInternalName() == val
                        || e.Email.NormalizeAsInternalName() == val)
                        .FirstOrDefault();

                        if (employee == null) throw new BadRequestException(GeneralCode.InvalidParams, $"Nhân viên {val} không tồn tại");

                        entity.SetPropertyValue(propertyName, employee.UserId);
                    }
                    return true;
                }
                return false;
            });


            // Kiểm tra duplicate
            var lstDuplicateData = lstData.GroupBy(t => new { t.EmployeeId, t.Date }).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

            if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied && lstDuplicateData.Count > 0)
            {
                var message = new StringBuilder("Thông tin chấm công bị trùng lặp: ");
                foreach (var duplicateData in lstDuplicateData)
                {
                    var employee = employees.First(e => e.UserId == duplicateData.EmployeeId);
                    message.Append($"NV {employee.EmployeeCode} ngày ${duplicateData.Date.UnixToDateTime().Value.ToString("dd/M/yyyy", CultureInfo.InvariantCulture)},");
                }
                message.Remove(message.Length - 1, 1);
                throw new BadRequestException(GeneralCode.InvalidParams, message.ToString());
            }

            var lstExistedData = _organizationDBContext.TimeSheet
                .Where(t => lstData.Any(d => d.EmployeeId == t.EmployeeId && d.Date.UnixToDateTime().Value == t.Date))
                .ToList();

            if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied && lstDuplicateData.Count > 0)
            {
                var message = new StringBuilder("Thông tin chấm công đã tồn tại: ");
                foreach (var duplicateData in lstDuplicateData)
                {
                    var employee = employees.First(e => e.UserId == duplicateData.EmployeeId);
                    message.Append($"NV {employee.EmployeeCode} ngày ${duplicateData.Date.UnixToDateTime().Value.ToString("dd/M/yyyy", CultureInfo.InvariantCulture)},");
                }
                message.Remove(message.Length - 1, 1);
                throw new BadRequestException(GeneralCode.InvalidParams, message.ToString());
            }

            foreach (var item in lstData)
            {
                var current = lstExistedData.Where(t => t.EmployeeId == item.EmployeeId && t.Date == item.Date.UnixToDateTime().Value).FirstOrDefault();
                if (current == null)
                {
                    current = new TimeSheet
                    {
                        EmployeeId = item.EmployeeId,
                        Date = item.Date.UnixToDateTime().Value,
                        TimeIn = TimeSpan.FromSeconds(item.TimeIn),
                        TimeOut = TimeSpan.FromSeconds(item.TimeOut)
                    };
                    _organizationDBContext.TimeSheet.Add(current);
                }
                else if (current != null && mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
                {
                    current.TimeIn = TimeSpan.FromSeconds(item.TimeIn);
                    current.TimeOut = TimeSpan.FromSeconds(item.TimeOut);
                }
            }
            _organizationDBContext.SaveChanges();

            return true;
        }

    }
}