
using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DynamicBill;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.Voucher
{
    public class VoucherTypeSimpleProjectMappingModel : VoucherTypeSimpleModel, IMapFrom<VoucherType>
    {

    }

    public class VoucherTypeModel : VoucherTypeSimpleProjectMappingModel, ITypeData
    {
        public VoucherTypeModel()
        {
        }

        public string PreLoadAction { get; set; }
        public string PostLoadAction { get; set; }
        public string AfterLoadAction { get; set; }
        public string BeforeSubmitAction { get; set; }
        public string BeforeSaveAction { get; set; }
        public string AfterSaveAction { get; set; }
        public string AfterUpdateRowsJsAction { get; set; }
        public bool IsHide { get; set; }
    }

    public class VoucherTypeFullModel : VoucherTypeExecData
    {
        public VoucherTypeFullModel()
        {
            VoucherAreas = new List<VoucherAreaModel>();
        }
        public ICollection<VoucherAreaModel> VoucherAreas { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<VoucherType, VoucherTypeFullModel>()
                .ForMember(dest => dest.VoucherAreas, opt => opt.MapFrom(src => src.VoucherArea));
        }
    }

    public class VoucherTypeExecData : VoucherTypeModel, ITypeExecData
    {
        private VoucherTypeGlobalSettingModel globalSetting { get; set; }
        private ExecCodeCombine<ITypeData> execCodeCombine;
        public VoucherTypeExecData()
        {
            execCodeCombine = new ExecCodeCombine<ITypeData>(this);
        }

        public void SetGlobalSetting(VoucherTypeGlobalSettingModel globalSetting)
        {
            this.globalSetting = globalSetting;
        }

        public string PreLoadActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.PreLoadAction), globalSetting);
        public string PostLoadActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.PostLoadAction), globalSetting);
        public string AfterLoadActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.AfterLoadAction), globalSetting);
        public string BeforeSubmitActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.BeforeSubmitAction), globalSetting);
        public string BeforeSaveActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.BeforeSaveAction), globalSetting);
        public string AfterSaveActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.AfterSaveAction), globalSetting);
        public string AfterUpdateRowsJsActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.AfterUpdateRowsJsAction), globalSetting);

    }
}
