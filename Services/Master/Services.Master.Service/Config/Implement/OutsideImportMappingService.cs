using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.OutsideMapping;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class OutsideImportMappingService : IOutsideImportMappingService
    {
        private MasterDBContext _masterDBContext;
        private IMapper _mapper;
        public OutsideImportMappingService(MasterDBContext masterDBContext, IMapper mapper)
        {
            _masterDBContext = masterDBContext;
            _mapper = mapper;
        }

        public async Task<PageData<OutsideMappingModelList>> GetList(string keyword, int page, int size)
        {
            var query = _masterDBContext.OutsideImportMappingFunction.AsQueryable();
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
            var existedFunction = await _masterDBContext.OutsideImportMappingFunction.FirstOrDefaultAsync(f => f.MappingFunctionKey == model.MappingFunctionKey);
            if (existedFunction != null) throw new BadRequestException(GeneralCode.InvalidParams, "Định danh chức năng đã tồn tại");

            using (var trans = await _masterDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var functionInfo = _mapper.Map<OutsideImportMappingFunction>(model);
                    await _masterDBContext.AddAsync(functionInfo);
                    await _masterDBContext.SaveChangesAsync();

                    var mappings = new List<OutsideImportMapping>();
                    foreach (var mapping in model.FieldMappings)
                    {
                        var mappingField = _mapper.Map<OutsideImportMapping>(mapping);
                        mappingField.OutsideImportMappingFunctionId = functionInfo.OutsideImportMappingFunctionId;

                        mappings.Add(mappingField);
                    }

                    await _masterDBContext.OutsideImportMapping.AddRangeAsync(mappings);
                    await _masterDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    return functionInfo.OutsideImportMappingFunctionId;
                }
                catch (Exception)
                {
                    await trans.TryRollbackTransactionAsync();
                    throw;
                }
            }

        }

        public async Task<OutsideMappingModel> GetImportMappingInfo(int outsideImportMappingFunctionId)
        {
            var functionInfo = await _masterDBContext.OutsideImportMappingFunction.FirstOrDefaultAsync(f => f.OutsideImportMappingFunctionId == outsideImportMappingFunctionId);

            return await GetInfo(functionInfo);
        }

        public async Task<OutsideMappingModel> GetImportMappingInfo(string mappingFunctionKey)
        {
            var functionInfo = await _masterDBContext.OutsideImportMappingFunction.FirstOrDefaultAsync(f => f.MappingFunctionKey == mappingFunctionKey);

            return await GetInfo(functionInfo);
        }

        private async Task<OutsideMappingModel> GetInfo(OutsideImportMappingFunction functionInfo)
        {
            if (functionInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy cấu hình của chức năng trong hệ thống!");

            var data = _mapper.Map<OutsideMappingModel>(functionInfo);

            var mappings = await _masterDBContext.OutsideImportMapping.Where(m => m.OutsideImportMappingFunctionId == functionInfo.OutsideImportMappingFunctionId).ToListAsync();

            data.FieldMappings = new List<OutsiteMappingModel>();
            foreach (var mapping in mappings)
            {
                var mappingField = _mapper.Map<OutsiteMappingModel>(mapping);
                data.FieldMappings.Add(mappingField);
            }
            return data;

        }

        public async Task<bool> UpdateImportMapping(int outsideImportMappingFunctionId, OutsideMappingModel model)
        {
            var existedFunction = await _masterDBContext.OutsideImportMappingFunction.FirstOrDefaultAsync(f => f.OutsideImportMappingFunctionId != outsideImportMappingFunctionId && f.MappingFunctionKey == model.MappingFunctionKey);
            if (existedFunction != null) throw new BadRequestException(GeneralCode.InvalidParams, "Định danh chức năng đã tồn tại");

            var functionInfo = await _masterDBContext.OutsideImportMappingFunction.FirstOrDefaultAsync(f => f.OutsideImportMappingFunctionId == outsideImportMappingFunctionId);

            if (functionInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy cấu hình của chức năng trong hệ thống!");

            using (var trans = await _masterDBContext.Database.BeginTransactionAsync())
            {
                try
                {

                    _mapper.Map(model, functionInfo);

                    var oldMappings = await _masterDBContext.OutsideImportMapping.Where(m => m.OutsideImportMappingFunctionId == outsideImportMappingFunctionId).ToListAsync();
                    foreach (var mapping in oldMappings)
                    {
                        mapping.IsDeleted = true;
                    }

                    var mappings = new List<OutsideImportMapping>();
                    foreach (var mapping in model.FieldMappings)
                    {
                        var mappingField = _mapper.Map<OutsideImportMapping>(mapping);
                        mappingField.OutsideImportMappingFunctionId = functionInfo.OutsideImportMappingFunctionId;
                        mappings.Add(mappingField);
                    }
                    await _masterDBContext.OutsideImportMapping.AddRangeAsync(mappings);
                    await _masterDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    return true;
                }
                catch (Exception)
                {
                    await trans.TryRollbackTransactionAsync();
                    throw;
                }
            }
        }

        public async Task<bool> DeleteImportMapping(int outsideImportMappingFunctionId)
        {
            var functionInfo = await _masterDBContext.OutsideImportMappingFunction.FirstOrDefaultAsync(f => f.OutsideImportMappingFunctionId == outsideImportMappingFunctionId);

            if (functionInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy cấu hình của chức năng trong hệ thống!");

            using (var trans = await _masterDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    functionInfo.IsDeleted = true;

                    var oldMappings = await _masterDBContext.OutsideImportMapping.Where(m => m.OutsideImportMappingFunctionId == outsideImportMappingFunctionId).ToListAsync();
                    foreach (var mapping in oldMappings)
                    {
                        mapping.IsDeleted = true;
                    }

                    await _masterDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    return true;
                }
                catch (Exception)
                {
                    await trans.TryRollbackTransactionAsync();
                    throw;
                }
            }
        }


        public async Task<OutsideImportMappingObjectModel> MappingObjectInfo(string mappingFunctionKey, string objectId)
        {
            var functionInfo = await _masterDBContext.OutsideImportMappingFunction.FirstOrDefaultAsync(f => f.MappingFunctionKey == mappingFunctionKey);
            if (functionInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy cấu hình của chức năng trong hệ thống!");

            var mappingObject = await _masterDBContext.OutsideImportMappingObject.FirstOrDefaultAsync(m => m.OutsideImportMappingFunctionId == functionInfo.OutsideImportMappingFunctionId && m.SourceId == objectId);

            if (mappingObject == null) return null;

            return new OutsideImportMappingObjectModel()
            {
                OutsideImportMappingFunctionId = mappingObject.OutsideImportMappingFunctionId,
                ObjectTypeId = (EnumObjectType)functionInfo.ObjectTypeId,
                InputTypeId = functionInfo.InputTypeId,
                SourceId = mappingObject.SourceId,
                InputBillFId = mappingObject.InputBillFId
            };
        }

        public async Task<bool> MappingObjectCreate(string mappingFunctionKey, string objectId, EnumObjectType billObjectTypeId, long billFId)
        {
            var functionInfo = await _masterDBContext.OutsideImportMappingFunction.FirstOrDefaultAsync(f => f.MappingFunctionKey == mappingFunctionKey);
            if (functionInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy cấu hình của chức năng trong hệ thống!");

            await _masterDBContext.OutsideImportMappingObject.AddAsync(new OutsideImportMappingObject()
            {
                OutsideImportMappingFunctionId = functionInfo.OutsideImportMappingFunctionId,
                SourceId = objectId,                
                InputBillFId = billFId
            });

            await _masterDBContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MappingObjectDelete(EnumObjectType billObjectTypeId, long billFId)
        {
            var data = _masterDBContext.OutsideImportMappingObject.Where(m => m.BillObjectTypeId == (int)billObjectTypeId && m.InputBillFId == billFId);
            _masterDBContext.OutsideImportMappingObject.RemoveRange(data);
            await _masterDBContext.SaveChangesAsync();
            return true;
        }
    }
}
