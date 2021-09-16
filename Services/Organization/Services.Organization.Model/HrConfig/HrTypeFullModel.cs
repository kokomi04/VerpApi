using System.Collections.Generic;
using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DynamicBill;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;

namespace Services.Organization.Model.HrConfig
{
    public class HrTypeFullModel: HrTypeExecData {
        public HrTypeFullModel()
        {
            HrAreas = new List<HrAreaModel>();
        }
        public ICollection<HrAreaModel> HrAreas { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMap<HrType, HrTypeFullModel>()
                .ForMember(dest => dest.HrAreas, opt => opt.MapFrom(src => src.HrArea));
        }
    }

    public class HrTypeExecData : HrTypeModel, ITypeExecData
    {
        public HrTypeGlobalSettingModel GlobalSetting { get; set; }
        private ExecCodeCombine<ITypeData> execCodeCombine;
        public HrTypeExecData()
        {
            execCodeCombine = new ExecCodeCombine<ITypeData>(this);
        }

        public string PreLoadActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.PreLoadAction), GlobalSetting);
        public string PostLoadActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.PostLoadAction), GlobalSetting);
        public string AfterLoadActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.AfterLoadAction), GlobalSetting);
        public string BeforeSubmitActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.BeforeSubmitAction), GlobalSetting);
        public string BeforeSaveActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.BeforeSaveAction), GlobalSetting);
        public string AfterSaveActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.AfterSaveAction), GlobalSetting);
        public string AfterUpdateRowsJsActionExec => execCodeCombine.GetExecCode(nameof(ITypeData.AfterUpdateRowsJsAction), GlobalSetting);
    }
}