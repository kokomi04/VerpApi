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
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountant.Model.Programing;

namespace VErp.Services.Accountant.Service.Programing.Implement
{
    /// <summary>
    /// Manage the programeing functions
    /// Do not need to log action on these
    /// </summary>
    public class ProgramingFunctionService : IProgramingFunctionService
    {
        private readonly AccountingDBContext _accountingDBContext;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly AppSetting _appSetting;
        private readonly IMapper _mapper;

        public ProgramingFunctionService(AccountingDBContext accountingDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProgramingFunctionService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            )
        {
            _accountingDBContext = accountingDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _appSetting = appSetting.Value;
            _mapper = mapper;
        }

        public async Task<PageData<ProgramingFunctionOutputList>> GetListFunctions(string keyword, EnumProgramingLang? programingLangId, EnumProgramingLevel? programingLevelId, int page, int size)
        {
            var query = _accountingDBContext.ProgramingFunction.AsQueryable();
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
            await _accountingDBContext.AddAsync(info);
            await _accountingDBContext.SaveChangesAsync();
            return info.ProgramingFunctionId;
        }

        public async Task<ProgramingFunctionModel> GetFunctionInfo(int programingFunctionId)
        {
            var info = await _accountingDBContext.ProgramingFunction.Where(f => f.ProgramingFunctionId == programingFunctionId).ProjectTo<ProgramingFunctionModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy function trong hệ thống");
            }
            return info;
        }

        public async Task<bool> UpdateFunction(int programingFunctionId, ProgramingFunctionModel model)
        {
            var info = await _accountingDBContext.ProgramingFunction.FirstOrDefaultAsync(f => f.ProgramingFunctionId == programingFunctionId);
            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy function trong hệ thống");
            }
            _mapper.Map(model, info);

            await _accountingDBContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFunction(int programingFunctionId)
        {
            var info = await _accountingDBContext.ProgramingFunction.FirstOrDefaultAsync(f => f.ProgramingFunctionId == programingFunctionId);
            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy function trong hệ thống");
            }

            _accountingDBContext.ProgramingFunction.Remove(info);

            await _accountingDBContext.SaveChangesAsync();
            return true;
        }
    }
}
