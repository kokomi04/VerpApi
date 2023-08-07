
using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DynamicBill;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.Input

{
    public interface IInputTypeExtra: ITypeData
    {
        public bool IsOpenning { get; set; }
        public bool IsParentAllowcation { get; set; }        
        public IList<int> DataAllowcationBillTypeIds { get; set; }
        public int? ResultAllowcationBillTypeId { get; set; }
        public bool IsHide { get; set; }
    }

    public class InputTypeSimpleProjectMappingModel : InputTypeSimpleModel, IMapFrom<InputType>
    {

    }

    public abstract class InputTypeModelAbstract : InputTypeSimpleProjectMappingModel, IInputTypeExtra
    {
        public InputTypeModelAbstract()
        {
        }


        public string PreLoadAction { get; set; }
        public string PostLoadAction { get; set; }
        public string AfterLoadAction { get; set; }
        public string BeforeSubmitAction { get; set; }
        public string BeforeSaveAction { get; set; }
        public string AfterSaveAction { get; set; }
        public string AfterUpdateRowsJsAction { get; set; }
        public string CalcResultAllowcationSqlQuery { get; set; }
        public bool IsOpenning { get; set; }
        public bool IsParentAllowcation { get; set; }
        public int? ParentId { get; set; }
        public IList<int> DataAllowcationBillTypeIds { get; set; }
        public int? ResultAllowcationBillTypeId { get; set; }
        public bool IsHide { get; set; }

        //public MenuStyleModel MenuStyle { get; set; }

        public void MappingBase<T>(Profile profile, Action<IMappingExpression<InputType, T>> mapSource = null, Action<IMappingExpression<T, InputType>> mapDestination = null) where T : InputTypeModelAbstract
        {
            var sourceMapping = profile.CreateMapCustom<InputType, T>()
                .ForMember(dest => dest.DataAllowcationBillTypeIds, opt => opt.MapFrom(src => src.DataAllowcationInputTypeIds == null ? null : src.DataAllowcationInputTypeIds.JsonDeserialize<int[]>()))
                .ForMember(dest => dest.ResultAllowcationBillTypeId, opt => opt.MapFrom(src => src.ResultAllowcationInputTypeId));
            if (mapSource != null)
            {
                mapSource(sourceMapping);
            };
            var destinationMapping = sourceMapping.ReverseMapCustom()
                .ForMember(dest => dest.DataAllowcationInputTypeIds, opt => opt.MapFrom(src => src.DataAllowcationBillTypeIds == null ? null : src.DataAllowcationBillTypeIds.JsonSerialize()))
                .ForMember(dest => dest.ResultAllowcationInputTypeId, opt => opt.MapFrom(src => src.ResultAllowcationBillTypeId));

            if (mapDestination != null)
                mapDestination(destinationMapping);
        }

        public abstract void Mapping(Profile profile);


    }

    public sealed class InputTypeModel : InputTypeSimpleProjectMappingModel, IInputTypeExtra, IMapFrom<InputType>
    {
        public string PreLoadAction { get; set; }
        public string PostLoadAction { get; set; }
        public string AfterLoadAction { get; set; }
        public string BeforeSubmitAction { get; set; }
        public string BeforeSaveAction { get; set; }
        public string AfterSaveAction { get; set; }
        public string AfterUpdateRowsJsAction { get; set; }
        public string CalcResultAllowcationSqlQuery { get; set; }
        public bool IsOpenning { get; set; }
        public bool IsParentAllowcation { get; set; }        
        public IList<int> DataAllowcationBillTypeIds { get; set; }
        public int? ResultAllowcationBillTypeId { get; set; }
        public bool IsHide { get; set; }

        public void Mapping(Profile profile)
        {
            var sourceMapping = profile.CreateMapCustom<InputType, InputTypeModel>()
                .ForMember(dest => dest.DataAllowcationBillTypeIds, opt => opt.MapFrom(src => src.DataAllowcationInputTypeIds == null ? null : src.DataAllowcationInputTypeIds.JsonDeserialize<int[]>()))
                .ForMember(dest => dest.ResultAllowcationBillTypeId, opt => opt.MapFrom(src => src.ResultAllowcationInputTypeId))
                .ReverseMapCustom()
                .ForMember(dest => dest.DataAllowcationInputTypeIds, opt => opt.MapFrom(src => src.DataAllowcationBillTypeIds == null ? null : src.DataAllowcationBillTypeIds.JsonSerialize()))
                .ForMember(dest => dest.ResultAllowcationInputTypeId, opt => opt.MapFrom(src => src.ResultAllowcationBillTypeId));

        }
    }

    public class InputTypeFullModel : InputTypeExecData
    {
        public InputTypeFullModel()
        {
            InputAreas = new List<InputAreaModel>();
        }
        public ICollection<InputAreaModel> InputAreas { get; set; }


        public override void Mapping(Profile profile)
        {
            MappingBase<InputTypeFullModel>(profile, map =>
            {
                map.ForMember(dest => dest.InputAreas, opt => opt.MapFrom(src => src.InputArea));
            }, map =>
            {
                map.ForMember(dest => dest.InputArea, opt => opt.MapFrom(src => src.InputAreas));
            });

        }
    }

    public class InputTypeExecData : InputTypeModelAbstract, ITypeExecData
    {
        private InputTypeGlobalSettingModel globalSetting;
        private ExecCodeCombine<ITypeData> execCodeCombine;
        public InputTypeExecData()
        {
            execCodeCombine = new ExecCodeCombine<ITypeData>(this);
        }
        public void SetGlobalSetting(InputTypeGlobalSettingModel globalSetting)
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

        public override void Mapping(Profile profile)
        {
            MappingBase<InputTypeExecData>(profile);
        }
    }

}
