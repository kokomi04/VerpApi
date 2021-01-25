using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum.Accountant;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountancy.Model.Programing;
using Microsoft.Data.SqlClient;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Commons.Library;
namespace VErp.Services.Accountancy.Service.Programing.Implement
{
    /// <summary>
    /// Manage the programeing functions
    /// Do not need to log action on these
    /// </summary>
    public class ProgramingFunctionService : IProgramingFunctionService
    {
        private readonly AccountancyDBContext _accountancyDBContext;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly AppSetting _appSetting;
        private readonly IMapper _mapper;

        public ProgramingFunctionService(AccountancyDBContext accountingDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProgramingFunctionService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            )
        {
            _accountancyDBContext = accountingDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _appSetting = appSetting.Value;
            _mapper = mapper;
        }

        public async Task<PageData<ProgramingFunctionOutputList>> GetListFunctions(string keyword, EnumProgramingLang? programingLangId, EnumProgramingLevel? programingLevelId, int page, int size)
        {
            var query = _accountancyDBContext.ProgramingFunction.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(f => f.ProgramingFunctionName.Contains(keyword));
            }

            if (programingLangId.HasValue)
            {
                query = query.Where(f => f.ProgramingLangId == (int)programingLangId.Value);
            }

            if (programingLevelId.HasValue)
            {
                query = query.Where(f => f.ProgramingLevelId == (int)programingLevelId.Value);
            }

            var total = await query.CountAsync();
            var lst = new List<ProgramingFunctionOutputList>();

            if (size > 0)
            {
                lst = await query.OrderBy(f => f.ProgramingFunctionName).Skip((page - 1) * size).Take(size).ProjectTo<ProgramingFunctionOutputList>(_mapper.ConfigurationProvider).ToListAsync();
            }
            else
            {
                lst = await query.OrderBy(f => f.ProgramingFunctionName).ProjectTo<ProgramingFunctionOutputList>(_mapper.ConfigurationProvider).ToListAsync();
            }

            return (lst, total);
        }

        public async Task<int> AddFunction(ProgramingFunctionModel model)
        {
            var info = _mapper.Map<ProgramingFunction>(model);
            await _accountancyDBContext.AddAsync(info);
            await _accountancyDBContext.SaveChangesAsync();
            return info.ProgramingFunctionId;
        }

        public async Task<ProgramingFunctionModel> GetFunctionInfo(int programingFunctionId)
        {
            var info = await _accountancyDBContext.ProgramingFunction.Where(f => f.ProgramingFunctionId == programingFunctionId).ProjectTo<ProgramingFunctionModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy function trong hệ thống");
            }
            return info;
        }

        public async Task<bool> UpdateFunction(int programingFunctionId, ProgramingFunctionModel model)
        {
            var info = await _accountancyDBContext.ProgramingFunction.FirstOrDefaultAsync(f => f.ProgramingFunctionId == programingFunctionId);
            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy function trong hệ thống");
            }
            _mapper.Map(model, info);

            await _accountancyDBContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFunction(int programingFunctionId)
        {
            var info = await _accountancyDBContext.ProgramingFunction.FirstOrDefaultAsync(f => f.ProgramingFunctionId == programingFunctionId);
            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy function trong hệ thống");
            }

            _accountancyDBContext.ProgramingFunction.Remove(info);

            await _accountancyDBContext.SaveChangesAsync();
            return true;
        }

        public async Task<IList<NonCamelCaseDictionary>> ExecSQLFunction(string programingFunctionName, NonCamelCaseDictionary<FuncParameter> inputData)
        {
            var function = _accountancyDBContext.ProgramingFunction.FirstOrDefault(f => f.ProgramingFunctionName == programingFunctionName);
            if (function == null) throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy chức năng {programingFunctionName}");

            List<SqlParameter> sqlParams = new List<SqlParameter>();
            if (inputData != null)
            {
                foreach (var item in inputData)
                {
                    sqlParams.Add(new SqlParameter($"@{item.Key}", item.Value != null && item.Value.Value != null ? item.Value.DataType.GetSqlValue(item.Value.Value) : DBNull.Value));
                }
            }

            var data = await _accountancyDBContext.QueryDataTable(function.FunctionBody, sqlParams);
            return data.ConvertData();
        }
    }
}
