using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.OutsideMapping;

namespace VErp.Services.Accountancy.Service.Input.Implement
{
    public class OutsideImportMappingService : IOutsideImportMappingService
    {
        private AccountancyDBContext _accountancyDBContext;
        private IMapper _mapper;
        public OutsideImportMappingService(AccountancyDBContext accountancyDBContext, IMapper mapper)
        {
            _accountancyDBContext = accountancyDBContext;
            _mapper = mapper;
        }

        public async Task<PageData<OutsideMappingModelList>> GetList(string keyword, int page, int size)
        {
            var query = _accountancyDBContext.OutsideImportMappingFunction.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(q => q.FunctionName.Contains(keyword) || q.MappingFunctionKey.Contains(keyword) || q.Description.Contains(keyword));
            }

            var total = await query.CountAsync();
            IList<OutsideMappingModelList> pagedData = null;
            if (size > 0)
            {
                pagedData = await query.OrderBy(q => q.FunctionName).Skip((page - 1) * size).Take(size).ProjectTo<OutsideMappingModelList>(_mapper.ConfigurationProvider).ToListAsync();
            }
            else
            {
                pagedData = await query.OrderBy(q => q.FunctionName).ProjectTo<OutsideMappingModelList>(_mapper.ConfigurationProvider).ToListAsync();
            }

            return (pagedData, total);
        }

        public async Task<int> CreateImportMapping(OutsideMappingModel model)
        {
            var existedFunction = _accountancyDBContext.OutsideImportMappingFunction.FirstOrDefaultAsync(f => f.MappingFunctionKey == model.MappingFunctionKey);
            if (existedFunction != null) throw new BadRequestException(GeneralCode.InvalidParams, "Định danh chức năng đã tồn tại");

            using (var trans = await _accountancyDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var functionInfo = _mapper.Map<OutsideImportMappingFunction>(model);
                    await _accountancyDBContext.AddAsync(functionInfo);
                    await _accountancyDBContext.SaveChangesAsync();

                    var mappings = new List<OutsideImportMapping>();
                    foreach (var mapping in model.FieldMappings)
                    {
                        var mappingField = _mapper.Map<OutsideImportMapping>(mapping);
                        mappingField.OutsideImportMappingFunctionId = functionInfo.OutsideImportMappingFunctionId;
                    }
                    await _accountancyDBContext.OutsideImportMapping.AddRangeAsync(mappings);
                    await _accountancyDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    return functionInfo.OutsideImportMappingFunctionId;
                }
                catch (Exception)
                {
                    await trans.RollbackAsync();
                    throw;
                }
            }

        }

        public async Task<OutsideMappingModel> GetImportMappingInfo(int outsideImportMappingFunctionId)
        {
            var functionInfo = await _accountancyDBContext.OutsideImportMappingFunction.FirstOrDefaultAsync(f => f.OutsideImportMappingFunctionId == outsideImportMappingFunctionId);

            return await GetInfo(functionInfo);
        }

        public async Task<OutsideMappingModel> GetImportMappingInfo(string mappingFunctionKey)
        {
            var functionInfo = await _accountancyDBContext.OutsideImportMappingFunction.FirstOrDefaultAsync(f => f.MappingFunctionKey == mappingFunctionKey);

            return await GetInfo(functionInfo);
        }

        private async Task<OutsideMappingModel> GetInfo(OutsideImportMappingFunction functionInfo)
        {
            if (functionInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy cấu hình của chức năng trong hệ thống!");

            var data = _mapper.Map<OutsideMappingModel>(functionInfo);

            var mappings = await _accountancyDBContext.OutsideImportMapping.Where(m => m.OutsideImportMappingFunctionId == functionInfo.OutsideImportMappingFunctionId).ToListAsync();

            foreach (var mapping in mappings)
            {
                var mappingField = _mapper.Map<OutsiteMappingModel>(mapping);
                data.FieldMappings.Add(mappingField);
            }
            return data;

        }

        public async Task<bool> UpdateImportMapping(int outsideImportMappingFunctionId, OutsideMappingModel model)
        {
            var existedFunction = _accountancyDBContext.OutsideImportMappingFunction.FirstOrDefaultAsync(f => f.OutsideImportMappingFunctionId != outsideImportMappingFunctionId && f.MappingFunctionKey == model.MappingFunctionKey);
            if (existedFunction != null) throw new BadRequestException(GeneralCode.InvalidParams, "Định danh chức năng đã tồn tại");

            var functionInfo = await _accountancyDBContext.OutsideImportMappingFunction.FirstOrDefaultAsync(f => f.OutsideImportMappingFunctionId == outsideImportMappingFunctionId);

            if (functionInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy cấu hình của chức năng trong hệ thống!");

            using (var trans = await _accountancyDBContext.Database.BeginTransactionAsync())
            {
                try
                {

                    _mapper.Map(model, functionInfo);

                    var oldMappings = await _accountancyDBContext.OutsideImportMapping.Where(m => m.OutsideImportMappingFunctionId == outsideImportMappingFunctionId).ToListAsync();
                    foreach (var mapping in oldMappings)
                    {
                        mapping.IsDeleted = true;
                    }

                    var mappings = new List<OutsideImportMapping>();
                    foreach (var mapping in model.FieldMappings)
                    {
                        var mappingField = _mapper.Map<OutsideImportMapping>(mapping);
                        mappingField.OutsideImportMappingFunctionId = functionInfo.OutsideImportMappingFunctionId;
                    }
                    await _accountancyDBContext.OutsideImportMapping.AddRangeAsync(mappings);
                    await _accountancyDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    return true;
                }
                catch (Exception)
                {
                    await trans.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> DeleteImportMapping(int outsideImportMappingFunctionId)
        {
            var functionInfo = await _accountancyDBContext.OutsideImportMappingFunction.FirstOrDefaultAsync(f => f.OutsideImportMappingFunctionId == outsideImportMappingFunctionId);

            if (functionInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy cấu hình của chức năng trong hệ thống!");

            using (var trans = await _accountancyDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    functionInfo.IsDeleted = true;

                    var oldMappings = await _accountancyDBContext.OutsideImportMapping.Where(m => m.OutsideImportMappingFunctionId == outsideImportMappingFunctionId).ToListAsync();
                    foreach (var mapping in oldMappings)
                    {
                        mapping.IsDeleted = true;
                    }

                    await _accountancyDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    return true;
                }
                catch (Exception)
                {
                    await trans.RollbackAsync();
                    throw;
                }
            }
        }
    }
}
