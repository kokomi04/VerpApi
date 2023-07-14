using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verp.Resources.Organization.Salary;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary.Implement
{
    public class SalaryRefTableService : ISalaryRefTableService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _salaryRefTableActivityLog;

        public SalaryRefTableService(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, IMapper mapper, IActivityLogService activityLogService)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
            _mapper = mapper;
            _salaryRefTableActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryRefTable);
        }

        public async Task<int> Create(SalaryRefTableModel model)
        {
            await Validate(0, model);

            var info = _mapper.Map<SalaryRefTable>(model);

            await _organizationDBContext.SalaryRefTable.AddAsync(info);
            await _organizationDBContext.SaveChangesAsync();
            await _salaryRefTableActivityLog.LogBuilder(() => SalaryRefTableActivityLogMessage.Create)
                .MessageResourceFormatDatas(model.RefTableCode)
                .ObjectId(info.SalaryRefTableId)
                .JsonData(info)
                .CreateLog();
            return info.SalaryRefTableId;
        }

        public async Task<bool> Delete(int salaryRefTableId)
        {
            var info = await _organizationDBContext.SalaryRefTable.FirstOrDefaultAsync(s => s.SalaryRefTableId == salaryRefTableId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            await ValidateRefTableUseInOtherLink(info.Alias);
            await ValidateRefTableUseInGroup(info.Alias);
            await ValidateRefTableUseInFields(info.Alias);


            _organizationDBContext.SalaryRefTable.Remove(info);
            await _organizationDBContext.SaveChangesAsync();
            await _salaryRefTableActivityLog.LogBuilder(() => SalaryRefTableActivityLogMessage.Delete)
                .MessageResourceFormatDatas(info.RefTableCode)
                .ObjectId(info.SalaryRefTableId)
                .JsonData(info)
                .CreateLog();
            return true;
        }



        public async Task<IList<SalaryRefTableModel>> GetList()
        {
            return await _organizationDBContext.SalaryRefTable.ProjectTo<SalaryRefTableModel>(_mapper.ConfigurationProvider)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
        }

        public async Task<bool> Update(int salaryRefTableId, SalaryRefTableModel model)
        {
            var info = await _organizationDBContext.SalaryRefTable.FirstOrDefaultAsync(s => s.SalaryRefTableId == salaryRefTableId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            await Validate(salaryRefTableId, model);

            _mapper.Map(model, info);
            info.SalaryRefTableId = salaryRefTableId;
            await _organizationDBContext.SaveChangesAsync();
            await _salaryRefTableActivityLog.LogBuilder(() => SalaryRefTableActivityLogMessage.Update)
                .MessageResourceFormatDatas(model.RefTableCode)
                .ObjectId(info.SalaryRefTableId)
                .JsonData(model)
                .CreateLog();
            return true;
        }

        private async Task Validate(int salaryRefTableId, SalaryRefTableModel model)
        {
            if (await _organizationDBContext.SalaryRefTable.AnyAsync(s => s.Alias == model.Alias && s.SalaryRefTableId != salaryRefTableId))
            {
                throw GeneralCode.InvalidParams.BadRequest($"Định danh {model.Alias} đã tồn tại, vui lòng chọn định danh khác!");
            }

        }

        private async Task ValidateRefTableUseInOtherLink(string alias)
        {
            var refTable = await _organizationDBContext.SalaryRefTable.FirstOrDefaultAsync(t => t.FromField.Contains(alias));
            if (refTable != null)
            {
                throw GeneralCode.InvalidParams.BadRequest($"Liên kết đang sử dụng để liên kết sang danh mục khác {refTable.Alias}!");
            }
        }

        private async Task ValidateRefTableUseInGroup(string alias)
        {
            var groups = await _organizationDBContext.SalaryGroup.ToListAsync();
            foreach (var group in groups)
            {
                var groupModel = _mapper.Map<SalaryGroupModel>(group);

                if (ContainText(groupModel.EmployeeFilter, alias + "."))
                {
                    throw GeneralCode.InvalidParams.BadRequest($"Bộ lọc loại bảng lương {group.Title} đang sử dụng thông tin liên kết!");
                }
            }
        }

        private async Task ValidateRefTableUseInFields(string alias)
        {
            var fields = await _organizationDBContext.SalaryField.ToListAsync();
            foreach (var field in fields)
            {
                var fieldModel = _mapper.Map<SalaryFieldModel>(field);
                foreach (var ex in fieldModel.Expression)
                {
                    if (ContainText(ex.Filter, alias + "."))
                    {
                        throw GeneralCode.InvalidParams.BadRequest($"Điều kiện thành phần bảng lương {field.Title} đang sử dụng thông tin liên kết!");
                    }

                    if (ex.ValueExpression?.Contains(alias + ".") == true)
                    {
                        throw GeneralCode.InvalidParams.BadRequest($"Giá trị thành phần bảng lương {field.Title} đang sử dụng thông tin liên kết!");
                    }

                }

            }
        }


        private bool ContainText(Clause clause, string text)
        {
            if (clause != null)
            {
                if (clause is SingleClause)
                {
                    var singleClause = clause as SingleClause;

                    if (singleClause.FieldName.Contains(text)) return true;
                    if (singleClause.Value?.ToString()?.Contains(text) == true) return true;
                    return false;
                }
                else if (clause is ArrayClause)
                {
                    var arrClause = clause as ArrayClause;

                    var res = new List<bool>();
                    for (int indx = 0; indx < arrClause.Rules.Count; indx++)
                    {
                        if (ContainText(arrClause.Rules.ElementAt(indx), text))
                        {
                            return true;
                        }
                    }

                }
                else
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin lọc không sai định dạng");
                }
            }
            return false;
        }

    }
}
