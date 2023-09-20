using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum.Accountant;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.ProgramingFunction;
using VErp.Infrastructure.EF.EFExtensions;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using VErp.Commons.Library;
namespace VErp.Services.Master.Service.ProgramingFunction.Implement
{
    public class ProgramingFunctionCustomService : IProgramingFunctionCustomService
    {
        private readonly IMapper _mapper;
        private readonly MasterDBContext _masterDbContext;

        public ProgramingFunctionCustomService(IMapper mapper, MasterDBContext masterDbContext)
        {
            _mapper = mapper;
            _masterDbContext = masterDbContext;
        }
        public async Task<int> AddFunction(ProgramingFunctionCustomModel model)
        {
            try
            {
                var info = _mapper.Map<ProgramingFunctionCustom>(model);
                await _masterDbContext.ProgramingFunctionCustom.AddAsync(info);
                await _masterDbContext.SaveChangesAsync();
                return info.ProgramingFunctionId;
            }
            catch (Exception ex)
            {

                throw new BadRequestException(ex.InnerException.Message);
            }
           
        }

        public async Task<bool> DeleteFunction(int programingFunctionId)
        {
            var info = await _masterDbContext.ProgramingFunctionCustom.FirstOrDefaultAsync(f => f.ProgramingFunctionId == programingFunctionId);
            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy function trong hệ thống");
            }

            _masterDbContext.ProgramingFunctionCustom.Remove(info);

            await _masterDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IList<NonCamelCaseDictionary>> ExecSQLFunction(string programingFunctionName, NonCamelCaseDictionary<FuncParameter> inputData)
        {
            var function = _masterDbContext.ProgramingFunctionCustom.FirstOrDefault(f => f.ProgramingFunctionName == programingFunctionName);
            if (function == null) throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy chức năng {programingFunctionName}");

            List<SqlParameter> sqlParams = new List<SqlParameter>();
            if (inputData != null)
            {
                foreach (var item in inputData)
                {
                    sqlParams.Add(new SqlParameter($"@{item.Key}", item.Value != null && item.Value.Value != null ? item.Value.DataType.GetSqlValue(item.Value.Value) : DBNull.Value));
                }
            }

            var data = await _masterDbContext.QueryDataTableRaw(function.FunctionBody, sqlParams);
            return data.ConvertData();
        }

        public async Task<ProgramingFunctionCustomModel> GetFunctionInfo(int programingFunctionId)
        {
            var info = await _masterDbContext.ProgramingFunctionCustom.Where(f => f.ProgramingFunctionId == programingFunctionId).ProjectTo<ProgramingFunctionCustomModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy function trong hệ thống");
            }
            return info;
        }

        public async Task<PageData<ProgramingFunctionCustomOutputList>> GetListFunctions(string keyword, EnumProgramingLang? programingLangId, EnumProgramingLevel? programingLevelId, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _masterDbContext.ProgramingFunctionCustom.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(f => f.ProgramingFunctionName.Contains(keyword) || f.FunctionBody.Contains(keyword) || f.Description.Contains(keyword) || f.Params.Contains(keyword));
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
            var lst = new List<ProgramingFunctionCustomOutputList>();

            if (size > 0)
            {
                lst = await query.OrderBy(f => f.ProgramingFunctionName).Skip((page - 1) * size).Take(size).ProjectTo<ProgramingFunctionCustomOutputList>(_mapper.ConfigurationProvider).ToListAsync();
            }
            else
            {
                lst = await query.OrderBy(f => f.ProgramingFunctionName).ProjectTo<ProgramingFunctionCustomOutputList>(_mapper.ConfigurationProvider).ToListAsync();
            }

            return (lst, total);
        }

        public async Task<bool> UpdateFunction(int programingFunctionId, ProgramingFunctionCustomModel model)
        {
            var info = await _masterDbContext.ProgramingFunctionCustom.FirstOrDefaultAsync(f => f.ProgramingFunctionId == programingFunctionId);
            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy function trong hệ thống");
            }
            _mapper.Map(model, info);

            await _masterDbContext.SaveChangesAsync();
            return true;
        }
    }
}
