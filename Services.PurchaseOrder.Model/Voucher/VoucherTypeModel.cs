
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DynamicBill;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.Voucher
{
    public class VoucherTypeSimpleProjectMappingModel : VoucherTypeSimpleModel, IMapFrom<VoucherType>
    {
       
    }

    public class VoucherTypeModel: VoucherTypeSimpleProjectMappingModel, ITypeData
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
        public string AfterInsertLinesJsAction { get; set; }
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
            profile.CreateMap<VoucherType, VoucherTypeFullModel>()
                .ForMember(dest => dest.VoucherAreas, opt => opt.MapFrom(src => src.VoucherArea));
        }
    }

    public class VoucherTypeExecData : VoucherTypeModel, ITypeExecData
    {
        public VoucherTypeGlobalSettingModel GlobalSetting { get; set; }
        private ExecCodeCombine<ITypeData> execCodeCombine;
        public VoucherTypeExecData()
        {
            execCodeCombine = new ExecCodeCombine<ITypeData>(this);
        }

        public string PreLoadActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.PreLoadAction), GlobalSetting);
        public string PostLoadActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.PostLoadAction), GlobalSetting);
        public string AfterLoadActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.AfterLoadAction), GlobalSetting);
        public string BeforeSubmitActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.BeforeSubmitAction), GlobalSetting);
        public string BeforeSaveActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.BeforeSaveAction), GlobalSetting);
        public string AfterSaveActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.AfterSaveAction), GlobalSetting);
        public string AfterInsertLinesJsActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.AfterInsertLinesJsAction), GlobalSetting);

    }
}
