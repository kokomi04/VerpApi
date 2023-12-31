﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Input
{
    public class InputTypeViewModelList : IMapFrom<InputTypeView>
    {
        public int InputTypeViewId { get; set; }
        public string InputTypeViewName { get; set; }
        public bool IsDefault { get; set; }
        public int Columns { get; set; }
        public int InputTypeGroupId { get; set; }
    }

    public class InputTypeViewModel : InputTypeViewModelList
    {
        public IList<InputTypeViewFieldModel> Fields { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<InputTypeView, InputTypeViewModel>()
                .ForMember(d => d.Fields, m => m.Ignore())
                .ReverseMap()
                .ForMember(d => d.InputTypeViewField, m => m.Ignore())
                .ForMember(d => d.InputType, m => m.Ignore());
        }
    }

    public class InputTypeViewFieldModel : IMapFrom<InputTypeViewField>
    {
        public int InputTypeViewFieldId { get; set; }
        public int Column { get; set; }
        public int SortOrder { get; set; }
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public EnumDataType DataTypeId { get; set; }
        public int DataSize { get; set; }
        public EnumFormType FormTypeId { get; set; }
        public string DefaultValue { get; set; }
        public IList<InputTypeViewFieldSelectFilter> SelectFilters { get; set; }
        public int? ReferenceCategoryId { get; set; }
        public int? ReferenceCategoryFieldId { get; set; }
        public int? ReferenceCategoryTitleFieldId { get; set; }
        public bool IsRequire { get; set; }
        public string RegularExpression { get; set; }

        public void Mapping(Profile profile) => profile.CreateMap<InputTypeViewField, InputTypeViewFieldModel>()
            .ForMember(m => m.DataTypeId, m => m.MapFrom(s => (EnumDataType)s.DataTypeId))
            .ForMember(m => m.FormTypeId, m => m.MapFrom(s => (EnumFormType)s.FormTypeId))
            .ForMember(m => m.SelectFilters, m => m.MapFrom(s => s.SelectFilters.JsonDeserialize<IList<InputTypeViewFieldSelectFilter>>()))
            .ReverseMap()
            .ForMember(m => m.InputTypeView, m => m.Ignore())
            .ForMember(m => m.DataType, m => m.Ignore())
            .ForMember(m => m.FormType, m => m.Ignore())
            .ForMember(m => m.ReferenceCategory, m => m.Ignore())
            .ForMember(m => m.ReferenceCategoryField, m => m.Ignore())
            .ForMember(m => m.ReferenceCategoryTitleField, m => m.Ignore())
            .ForMember(m => m.DataTypeId, m => m.MapFrom(s => (int)s.DataTypeId))
            .ForMember(m => m.FormTypeId, m => m.MapFrom(s => (int)s.FormTypeId))
            .ForMember(m => m.SelectFilters, m => m.MapFrom(s => s.SelectFilters.JsonSerialize()));
    }

    public class InputTypeViewFieldSelectFilter
    {
        public EnumLogicOperator LogicOperatorId { get; set; }
        public EnumOperator OperatorId { get; set; }

        public int InputAreaFieldId { get; set; }
    }
}
