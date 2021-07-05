
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DynamicBill;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.Input

{
    public class InputTypeSimpleProjectMappingModel : InputTypeSimpleModel, IMapFrom<InputType>
    {

    }

    public class InputTypeModel : InputTypeSimpleProjectMappingModel, ITypeData
    {
        public InputTypeModel()
        {
        }


        public string PreLoadAction { get; set; }
        public string PostLoadAction { get; set; }
        public string AfterLoadAction { get; set; }
        public string BeforeSubmitAction { get; set; }
        public string BeforeSaveAction { get; set; }
        public string AfterSaveAction { get; set; }
        public string AfterUpdateRowsJsAction { get; set; }

        //public MenuStyleModel MenuStyle { get; set; }
    }

    public class InputTypeFullModel : InputTypeExecData
    {
        public InputTypeFullModel()
        {
            InputAreas = new List<InputAreaModel>();
        }
        public ICollection<InputAreaModel> InputAreas { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMap<InputType, InputTypeFullModel>()
                .ForMember(dest => dest.InputAreas, opt => opt.MapFrom(src => src.InputArea));
        }
    }

    public class InputTypeExecData : InputTypeModel, ITypeExecData
    {
        public InputTypeGlobalSettingModel GlobalSetting { get; set; }
        private ExecCodeCombine<ITypeData> execCodeCombine;
        public InputTypeExecData()
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
