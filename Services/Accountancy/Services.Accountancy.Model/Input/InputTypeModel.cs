
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.Input

{
    public class InputTypeSimpleProjectMappingModel : InputTypeSimpleModel, IMapFrom<InputType>
    {

    }

    public class InputTypeModel : InputTypeSimpleProjectMappingModel
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
        public string AfterInsertLinesJsAction { get; set; }

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

    public class InputTypeExecData : InputTypeModel, IInputTypeExecData
    {
        public InputTypeGlobalSettingModel GlobalSetting { get; set; }

        public string PreLoadActionExec => string.IsNullOrWhiteSpace(PreLoadAction) ? GlobalSetting?.PreLoadAction : PreLoadAction.Replace(AccountantConstants.SUPER, GlobalSetting?.PreLoadAction);
        public string PostLoadActionExec => string.IsNullOrWhiteSpace(PostLoadAction) ? GlobalSetting?.PostLoadAction : PostLoadAction.Replace(AccountantConstants.SUPER, GlobalSetting?.PostLoadAction);
        public string AfterLoadActionExec => string.IsNullOrWhiteSpace(AfterLoadAction) ? GlobalSetting?.AfterLoadAction : AfterLoadAction.Replace(AccountantConstants.SUPER, GlobalSetting?.AfterLoadAction);
        public string BeforeSubmitActionExec => string.IsNullOrWhiteSpace(BeforeSubmitAction) ? GlobalSetting?.BeforeSubmitAction : BeforeSubmitAction.Replace(AccountantConstants.SUPER, GlobalSetting?.BeforeSubmitAction);
        public string BeforeSaveActionExec => string.IsNullOrWhiteSpace(BeforeSaveAction) ? GlobalSetting?.BeforeSaveAction : BeforeSaveAction.Replace(AccountantConstants.SUPER, GlobalSetting?.BeforeSaveAction);
        public string AfterSaveActionExec => string.IsNullOrWhiteSpace(AfterSaveAction) ? GlobalSetting?.AfterSaveAction : AfterSaveAction.Replace(AccountantConstants.SUPER, GlobalSetting?.AfterSaveAction);
        public string AfterInsertLinesJsActionExec => string.IsNullOrWhiteSpace(AfterInsertLinesJsAction) ? GlobalSetting?.AfterInsertLinesJsAction : AfterInsertLinesJsAction.Replace(AccountantConstants.SUPER, GlobalSetting?.AfterInsertLinesJsAction);

    }

    public interface IInputTypeExecData
    {
        string Title { get; }
        string PreLoadActionExec { get; }
        string PostLoadActionExec { get; }
        string AfterLoadActionExec { get; }
        string BeforeSubmitActionExec { get; }
        string BeforeSaveActionExec { get; }
        string AfterSaveActionExec { get; }
        string AfterInsertLinesJsActionExec { get; }

    }
}
