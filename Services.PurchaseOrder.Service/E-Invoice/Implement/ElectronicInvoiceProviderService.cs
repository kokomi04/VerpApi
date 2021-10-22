using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.E_Invoice;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model.E_Invoice;
using VErp.Commons.Library;
using Verp.Resources.PurchaseOrder.EInvoice;

namespace VErp.Services.PurchaseOrder.Service.E_Invoice.Implement
{
   

    public class ElectronicInvoiceProviderService : IElectronicInvoiceProviderService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _objectActivityLog;

        public ElectronicInvoiceProviderService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , IActivityLogService activityLogService
            , IMapper mapper
           )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _mapper = mapper;
            _objectActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ElectronicInvoiceProvider);
        }

        public async Task<IList<ElectronicInvoiceProviderModel>> GetList()
        {
            var lst = await _purchaseOrderDBContext.ElectronicInvoiceProvider.ToListAsync();
            
            return _mapper.Map<List<ElectronicInvoiceProviderModel>>(lst);
        }

        public async Task<ElectronicInvoiceProviderModel> GetInfo(EnumElectronicInvoiceProvider electronicInvoiceProviderId)
        {
            var info = await _purchaseOrderDBContext.ElectronicInvoiceProvider.FirstOrDefaultAsync(e => e.ElectronicInvoiceProviderId == (int)electronicInvoiceProviderId);
            if (info == null) throw GeneralCode.ItemNotFound.BadRequest();

            return _mapper.Map<ElectronicInvoiceProviderModel>(info);
        }

        public async Task<bool> Update(EnumElectronicInvoiceProvider electronicInvoiceProviderId, ElectronicInvoiceProviderModel model)
        {
            var info = await _purchaseOrderDBContext.ElectronicInvoiceProvider.FirstOrDefaultAsync(e => e.ElectronicInvoiceProviderId == (int)electronicInvoiceProviderId);
            if (info == null) throw GeneralCode.ItemNotFound.BadRequest();
            model.ElectronicInvoiceProviderId = electronicInvoiceProviderId;
            _mapper.Map(model, info);
            await _purchaseOrderDBContext.SaveChangesAsync();

            await _objectActivityLog.LogBuilder(() => ElectronicInvoiceProviderActivityLogMessage.Update)
                .MessageResourceFormatDatas(electronicInvoiceProviderId.ToString())
                .ObjectId((int)electronicInvoiceProviderId)
                .JsonData(model.JsonSerialize())
                .CreateLog();
            return true;
        }
    }
}
