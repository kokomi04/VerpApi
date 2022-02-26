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
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Service.TimeKeeping
{
    public interface ITimeSheetRawService
    {
        Task<long> AddTimeSheetRaw(TimeSheetRawModel model);
        Task<bool> DeleteTimeSheetRaw(long timeSheetRawId);
        Task<IList<TimeSheetRawModel>> GetListTimeSheetRaw();
        Task<TimeSheetRawModel> GetTimeSheetRaw(long timeSheetRawId);
        Task<bool> UpdateTimeSheetRaw(long timeSheetRawId, TimeSheetRawModel model);


        Task<CategoryNameModel> GetFieldDataForMapping();

        Task<bool> ImportTimeSheetRawFromMapping(ImportExcelMapping mapping, Stream stream);
    }

    public class TimeSheetRawService : ITimeSheetRawService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;

        public TimeSheetRawService(OrganizationDBContext organizationDBContext, IMapper mapper)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
        }

        public async Task<long> AddTimeSheetRaw(TimeSheetRawModel model)
        {
            var entity = _mapper.Map<TimeSheetRaw>(model);

            await _organizationDBContext.TimeSheetRaw.AddAsync(entity);
            await _organizationDBContext.SaveChangesAsync();

            return entity.TimeSheetRawId;
        }

        public async Task<bool> UpdateTimeSheetRaw(long timeSheetRawId, TimeSheetRawModel model)
        {
            var timeSheetRaw = await _organizationDBContext.TimeSheetRaw.FirstOrDefaultAsync(x => x.TimeSheetRawId == timeSheetRawId);
            if (timeSheetRaw == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            model.TimeSheetRawId = timeSheetRawId;
            _mapper.Map(model, timeSheetRaw);

            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteTimeSheetRaw(long timeSheetRawId)
        {
            var timeSheetRaw = await _organizationDBContext.TimeSheetRaw.FirstOrDefaultAsync(x => x.TimeSheetRawId == timeSheetRawId);
            if (timeSheetRaw == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            timeSheetRaw.IsDeleted = true;
            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<TimeSheetRawModel> GetTimeSheetRaw(long timeSheetRawId)
        {
            var timeSheetRaw = await _organizationDBContext.TimeSheetRaw.FirstOrDefaultAsync(x => x.TimeSheetRawId == timeSheetRawId);
            if (timeSheetRaw == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            return _mapper.Map<TimeSheetRawModel>(timeSheetRaw);
        }

        public async Task<IList<TimeSheetRawModel>> GetListTimeSheetRaw()
        {
            var query = _organizationDBContext.TimeSheetRaw.AsNoTracking();

            return await query.Select(x => new TimeSheetRawModel
            {
                Date = x.Date.GetUnix(),
                Time = x.Time.TotalSeconds,
                EmployeeId = x.EmployeeId,
                TimeSheetRawId = x.TimeSheetRawId
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

        public async Task<bool> ImportTimeSheetRawFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            var employees = await _organizationDBContext.Employee.ToListAsync();

            var lstData = reader.ReadSheetEntity<TimeSheetRawImportFieldModel>(mapping, (entity, propertyName, value) =>
            {

                if (propertyName == nameof(TimeSheetRawImportFieldModel.EmployeeId))
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

            foreach (var item in lstData)
            {
                var ent = new TimeSheetRaw
                {
                    EmployeeId = item.EmployeeId,
                    Date = item.Date.UnixToDateTime().Value,
                    Time = TimeSpan.FromSeconds(item.Time)
                };
                _organizationDBContext.TimeSheetRaw.Add(ent);
            }
            _organizationDBContext.SaveChanges();

            return true;
        }
    }
}