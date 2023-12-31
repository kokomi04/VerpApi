﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.ProgramingFunction;
using VErp.Infrastructure.EF.EFExtensions;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using VErp.Commons.Library;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.System;
using VErp.Commons.GlobalObject.InternalDataInterface.System;

namespace VErp.Services.Master.Service.ProgramingFunction.Implement
{
    public class UserProgramingFunctionService : IUserProgramingFunctionService
    {
        private readonly IMapper _mapper;
        private readonly MasterDBContext _masterDbContext;
        private readonly IProgramingFunctionHelperService _programingFunctionHelperService;

        public UserProgramingFunctionService(IMapper mapper, MasterDBContext masterDbContext, IProgramingFunctionHelperService programingFunctionHelperService)
        {
            _mapper = mapper;
            _masterDbContext = masterDbContext;
            _programingFunctionHelperService = programingFunctionHelperService;
        }
        public async Task<int> AddFunction(UserProgramingFunctionModel model)
        {
            var programingFunctions = await _masterDbContext.UserProgramingFunction.ToListAsync();
            if (programingFunctions.Any(x=> x.ProgramingFunctionName.ToLower() == model.ProgramingFunctionName.ToLower()))
            {
                throw new BadRequestException("Tên hàm đã tồn tại, xin vui lòng chọn tên khác!");
            }
            var info = _mapper.Map<UserProgramingFunction>(model);
            await _masterDbContext.UserProgramingFunction.AddAsync(info);
            await _masterDbContext.SaveChangesAsync();
            return info.UserProgramingFunctionId;
        }

        public async Task<bool> DeleteFunction(int userProgramingFunctionId)
        {
            var info = await _masterDbContext.UserProgramingFunction.FirstOrDefaultAsync(f => f.UserProgramingFunctionId == userProgramingFunctionId);
            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy function trong hệ thống");
            }

            _masterDbContext.UserProgramingFunction.Remove(info);

            await _masterDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IList<NonCamelCaseDictionary>> ExecSQLFunction(string programingFunctionName, NonCamelCaseDictionary<FuncParameter> inputData)
        {
            var function = _masterDbContext.UserProgramingFunction.FirstOrDefault(f => f.ProgramingFunctionName == programingFunctionName);
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

        public async Task<UserProgramingFunctionModel> GetFunctionInfo(int userProgramingFunctionId)
        {
            var info = await _masterDbContext.UserProgramingFunction.Where(f => f.UserProgramingFunctionId == userProgramingFunctionId).ProjectTo<UserProgramingFunctionModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy function trong hệ thống");
            }
            return info;
        }

        public async Task<PageData<UserProgramingFunctionOutputList>> GetListFunctions(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _masterDbContext.UserProgramingFunction.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(f => f.ProgramingFunctionName.Contains(keyword) || f.FunctionBody.Contains(keyword) || f.Description.Contains(keyword) || f.Params.Contains(keyword));
            }

            var total = await query.CountAsync();
            var lst = new List<UserProgramingFunctionOutputList>();

            if (size > 0)
            {
                lst = await query.OrderBy(f => f.ProgramingFunctionName).Skip((page - 1) * size).Take(size).ProjectTo<UserProgramingFunctionOutputList>(_mapper.ConfigurationProvider).ToListAsync();
            }
            else
            {
                lst = await query.OrderBy(f => f.ProgramingFunctionName).ProjectTo<UserProgramingFunctionOutputList>(_mapper.ConfigurationProvider).ToListAsync();
            }

            return (lst, total);
        }
        public async Task<PageData<ProgramingFunctionBaseModel>> GetAllSqls(string keyword)
        {
            var lst = (await _programingFunctionHelperService.GetAllSqls());
            return (lst, lst.Count);
        }
        public async Task<bool> UpdateFunction(int userProgramingFunctionId, UserProgramingFunctionModel model)
        {
            var info = await _masterDbContext.UserProgramingFunction.FirstOrDefaultAsync(f => f.UserProgramingFunctionId == userProgramingFunctionId);
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
