using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.Voucher;

namespace VErp.Services.PurchaseOrder.Service.Voucher
{
    public interface IVoucherConfigService
    {
        // Input type
        Task<IList<VoucherTypeFullModel>> GetAllVoucherTypes();

        Task<VoucherTypeFullModel> GetVoucherType(int voucherTypeId);
        Task<VoucherTypeFullModel> GetVoucherType(string inputTypeCode);

        Task<PageData<VoucherTypeModel>> GetVoucherTypes(string keyword, int page, int size);
        Task<IList<VoucherTypeSimpleModel>> GetVoucherTypeSimpleList();

        Task<VoucherTypeGlobalSettingModel> GetVoucherGlobalSetting();
        Task<bool> UpdateVoucherGlobalSetting(VoucherTypeGlobalSettingModel data);

        Task<int> AddVoucherType(VoucherTypeModel data);
        Task<bool> UpdateVoucherType(int voucherTypeId, VoucherTypeModel data);
        Task<bool> DeleteVoucherType(int voucherTypeId);
        Task<int> CloneVoucherType(int voucherTypeId);


        Task<int> VoucherTypeViewCreate(int voucherTypeId, VoucherTypeViewModel model);
        Task<bool> VoucherTypeViewUpdate(int voucherTypeViewId, VoucherTypeViewModel model);
        Task<bool> VoucherTypeViewDelete(int voucherTypeViewId);
        Task<IList<VoucherTypeViewModelList>> VoucherTypeViewList(int voucherTypeId);
        Task<VoucherTypeBasicOutput> GetVoucherTypeBasicInfo(int voucherTypeId);
        Task<VoucherTypeViewModel> GetVoucherTypeViewInfo(int voucherTypeId, int voucherTypeViewId);

        Task<int> VoucherTypeGroupCreate(VoucherTypeGroupModel model);
        Task<bool> VoucherTypeGroupUpdate(int voucherTypeGroupId, VoucherTypeGroupModel model);
        Task<bool> VoucherTypeGroupDelete(int voucherTypeGroupId);
        Task<IList<VoucherTypeGroupList>> VoucherTypeGroupList();

        // Area
        Task<VoucherAreaModel> GetVoucherArea(int voucherTypeId, int voucherAreaId);
        Task<PageData<VoucherAreaModel>> GetVoucherAreas(int voucherTypeId, string keyword, int page, int size);
        Task<int> AddVoucherArea(int voucherTypeId, VoucherAreaInputModel data);
        Task<bool> UpdateVoucherArea(int voucherTypeId, int voucherAreaId, VoucherAreaInputModel data);
        Task<bool> DeleteVoucherArea(int voucherTypeId, int voucherAreaId);

        // Field
        Task<PageData<VoucherFieldOutputModel>> GetVoucherFields(string keyword, int page, int size);
        Task<VoucherFieldInputModel> AddVoucherField(VoucherFieldInputModel data);
        Task<VoucherFieldInputModel> UpdateVoucherField(int voucherFieldId, VoucherFieldInputModel data);
        Task<bool> DeleteVoucherField(int voucherFieldId);
        Task<VoucherAreaFieldOutputFullModel> GetVoucherAreaField(int voucherTypeId, int voucherAreaId, int voucherAreaFieldId);
        Task<PageData<VoucherAreaFieldOutputFullModel>> GetVoucherAreaFields(int voucherTypeId, int voucherAreaId, string keyword, int page, int size);
        Task<bool> UpdateMultiField(int voucherTypeId, List<VoucherAreaFieldInputModel> fields);

    }
}
