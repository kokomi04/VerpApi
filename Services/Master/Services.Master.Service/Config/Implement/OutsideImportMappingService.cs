using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Master.Config.OutsideImportMapping;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.OutsideMapping;
using static Verp.Resources.Master.Config.OutsideImportMapping.OutsideImportMappingValidationMessage;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class OutsideImportMappingService : IOutsideImportMappingService
    {
        private MasterDBContext _masterDBContext;
        private IMapper _mapper;

        private readonly ObjectActivityLogFacade _outsideImportMappingActivityLog;

        public OutsideImportMappingService(MasterDBContext masterDBContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _masterDBContext = masterDBContext;
            _mapper = mapper;
            _outsideImportMappingActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.OutsideImportMappingFunction);
        }

        public async Task<PageData<OutsideMappingModelList>> GetList(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _masterDBContext.OutsideImportMappingFunction.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(q => q.FunctionName.Contains(keyword) || q.MappingFunctionKey.Contains(keyword) || q.Description.Contains(keyword));
            }

            var total = await query.CountAsync();
            IList<OutsideImportMappingFunction> pagedData = null;
            if (size > 0)
            {
                pagedData = await query.OrderBy(q => q.FunctionName).Skip((page - 1) * size).Take(size).ToListAsync();
            }
            else
            {
                pagedData = await query.OrderBy(q => q.FunctionName).ToListAsync();
            }

            return (_mapper.Map<IList<OutsideMappingModelList>>(pagedData), total);
        }

        public async Task<int> CreateImportMapping(OutsideMappingModel model)
        {
            var existedFunction = await _masterDBContext.OutsideImportMappingFunction.FirstOrDefaultAsync(f => f.MappingFunctionKey == model.MappingFunctionKey);
            if (existedFunction != null) throw MappingFunctionKeyAlreadyExisted.BadRequest();

            using (var trans = await _masterDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var functionInfo = _mapper.Map<OutsideImportMappingFunction>(model);
                    await _masterDBContext.OutsideImportMappingFunction.AddAsync(functionInfo);
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

                    await _outsideImportMappingActivityLog.LogBuilder(() => OutsideImportMappingActivityLogMessage.Create)
                        .MessageResourceFormatDatas(model.FunctionName)
                         .ObjectId(functionInfo.OutsideImportMappingFunctionId)
                         .JsonData(model.JsonSerialize())
                         .CreateLog();

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
            if (functionInfo == null) throw MappingFunctionNotFound.BadRequest();

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
            if (existedFunction != null) throw MappingFunctionKeyAlreadyExisted.BadRequest();

            var functionInfo = await _masterDBContext.OutsideImportMappingFunction.FirstOrDefaultAsync(f => f.OutsideImportMappingFunctionId == outsideImportMappingFunctionId);

            if (functionInfo == null) throw MappingFunctionNotFound.BadRequest();

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

                    await _outsideImportMappingActivityLog.LogBuilder(() => OutsideImportMappingActivityLogMessage.Update)
                      .MessageResourceFormatDatas(model.FunctionName)
                      .ObjectId(outsideImportMappingFunctionId)
                      .JsonData(model.JsonSerialize())
                      .CreateLog();

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

            if (functionInfo == null) throw MappingFunctionNotFound.BadRequest();

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

                    await _outsideImportMappingActivityLog.LogBuilder(() => OutsideImportMappingActivityLogMessage.Delete)
                      .MessageResourceFormatDatas(functionInfo.FunctionName)
                      .ObjectId(outsideImportMappingFunctionId)
                      .JsonData(functionInfo.JsonSerialize())
                      .CreateLog();

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
            if (functionInfo == null) throw MappingFunctionNotFound.BadRequest();

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
            if (functionInfo == null) throw MappingFunctionNotFound.BadRequest();

            await _masterDBContext.OutsideImportMappingObject.AddAsync(new OutsideImportMappingObject()
            {
                OutsideImportMappingFunctionId = functionInfo.OutsideImportMappingFunctionId,
                SourceId = objectId,
                BillObjectTypeId = (int)billObjectTypeId,
                InputBillFId = billFId
            });

            await _masterDBContext.SaveChangesAsync();

            await _outsideImportMappingActivityLog.LogBuilder(() => OutsideImportMappingActivityLogMessage.CreateMappingObject)
                  .MessageResourceFormatDatas(functionInfo.FunctionName, billObjectTypeId.GetEnumDescription())
                  .ObjectId(functionInfo.OutsideImportMappingFunctionId)
                  .JsonData(new
                  {
                      mappingFunctionKey,
                      objectId,
                      billObjectTypeId,
                      billFId
                  }.JsonSerialize())
                  .CreateLog();

            return true;
        }

        public async Task<bool> MappingObjectDelete(EnumObjectType billObjectTypeId, long billFId)
        {
            var data = _masterDBContext.OutsideImportMappingObject.Where(m => m.BillObjectTypeId == (int)billObjectTypeId && m.InputBillFId == billFId);
            _masterDBContext.OutsideImportMappingObject.RemoveRange(data);
            await _masterDBContext.SaveChangesAsync();

            await _outsideImportMappingActivityLog.LogBuilder(() => OutsideImportMappingActivityLogMessage.DeleteMappingObject)
                .MessageResourceFormatDatas(billObjectTypeId.GetEnumDescription())
                .ObjectType(billObjectTypeId)
                .ObjectId(billFId)
                .JsonData(new
                {
                    billObjectTypeId,
                    billFId
                }.JsonSerialize())
                .CreateLog();
            return true;
        }
    }
}
