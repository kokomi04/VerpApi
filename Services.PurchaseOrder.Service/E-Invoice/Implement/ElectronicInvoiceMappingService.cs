using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Verp.Resources.PurchaseOrder.EInvoice;
using VErp.Commons.Enums.ErrorCodes.PO;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model.E_Invoice;

namespace VErp.Services.PurchaseOrder.Service.E_Invoice.Implement
{
    public class ElectronicInvoiceMappingService : IElectronicInvoiceMappingService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _objectActivityLog;

        public ElectronicInvoiceMappingService(PurchaseOrderDBContext purchaseOrderDBContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _mapper = mapper;
            _objectActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ElectronicInvoiceMapping);
        }

        public async Task<IList<ElectronicInvoiceMappingModel>> GetList()
        {
            var lst = await _purchaseOrderDBContext.ElectronicInvoiceMapping.ToListAsync();

            return _mapper.Map<List<ElectronicInvoiceMappingModel>>(lst);
        }

        public async Task<ElectronicInvoiceMappingModel> GetInfo(int electronicInvoiceMappingId)
        {
            var info = await _purchaseOrderDBContext.ElectronicInvoiceMapping.FirstOrDefaultAsync(x => x.ElectronicInvoiceMappingId == electronicInvoiceMappingId);

            return _mapper.Map<ElectronicInvoiceMappingModel>(info);
        }

        public async Task<int> AddElectronicInvoiceMapping(ElectronicInvoiceMappingModel model)
        {
            var existsEntity = await _purchaseOrderDBContext.ElectronicInvoiceMapping.FirstOrDefaultAsync(x => x.ElectronicInvoiceProviderId == model.ElectronicInvoiceFunctionId
            && x.ElectronicInvoiceFunctionId == model.ElectronicInvoiceFunctionId
            && x.VoucherTypeId == model.VoucherTypeId);

            if (existsEntity != null)
                throw ElectronicInvoiceMappingErrorCode.ExistsElectronicInvoiceMapping.BadRequest();

            var entity = _mapper.Map<ElectronicInvoiceMapping>(model);

            await _purchaseOrderDBContext.ElectronicInvoiceMapping.AddAsync(entity);
            await _purchaseOrderDBContext.SaveChangesAsync();

            await _objectActivityLog.LogBuilder(() => ElectronicInvoiceMappingActivityLogMessage.Add)
               .MessageResourceFormatDatas(entity.ElectronicInvoiceMappingId)
               .ObjectId(entity.ElectronicInvoiceMappingId)
               .JsonData(model)
               .CreateLog();
            return entity.ElectronicInvoiceMappingId;
        }

        public async Task<bool> UpdateElectronicInvoiceMapping(int electronicInvoiceMappingId, ElectronicInvoiceMappingModel model)
        {
            var existsEntity = await _purchaseOrderDBContext.ElectronicInvoiceMapping.FirstOrDefaultAsync(x => x.ElectronicInvoiceMappingId == electronicInvoiceMappingId);

            if (existsEntity == null)
                throw GeneralCode.ItemNotFound.BadRequest();

            _mapper.Map(model, existsEntity);

            await _purchaseOrderDBContext.SaveChangesAsync();

            await _objectActivityLog.LogBuilder(() => ElectronicInvoiceMappingActivityLogMessage.Update)
               .MessageResourceFormatDatas(electronicInvoiceMappingId)
               .ObjectId(electronicInvoiceMappingId)
               .JsonData(model)
               .CreateLog();
            return true;
        }

        public async Task<bool> DeleteElectronicInvoiceMapping(int electronicInvoiceMappingId)
        {
            var existsEntity = await _purchaseOrderDBContext.ElectronicInvoiceMapping.FirstOrDefaultAsync(x => x.ElectronicInvoiceMappingId == electronicInvoiceMappingId);

            if (existsEntity == null)
                throw GeneralCode.ItemNotFound.BadRequest();

            existsEntity.IsDeleted = true;

            await _purchaseOrderDBContext.SaveChangesAsync();

            await _objectActivityLog.LogBuilder(() => ElectronicInvoiceMappingActivityLogMessage.Delete)
               .MessageResourceFormatDatas(electronicInvoiceMappingId)
               .ObjectId(electronicInvoiceMappingId)
               .JsonData(existsEntity)
               .CreateLog();

            return true;
        }
    }
}