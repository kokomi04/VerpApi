using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.Input
{
    public class InputTypeBasicOutput : IMapFrom<InputType>
    {
        public string Title { get; set; }
        public string InputTypeCode { get; set; }
        public IList<InputAreaBasicOutput> Areas { get; set; }
        public IList<InputTypeViewModelList> Views { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<InputType, InputTypeBasicOutput>()
                .ForMember(d => d.Areas, m => m.Ignore())
                .ForMember(d => d.Views, m => m.Ignore());
        }
    }

    public class InputAreaBasicOutput : IMapFrom<InputArea>
    {
        public int InputAreaId { get; set; }
        public string Title { get; set; }
        public string InputAreaCode { get; set; }
        public bool IsMultiRow { get; set; }
        public int Columns { get; set; }
        public string ColumnStyles { get; set; }
        public IList<InputAreaFieldBasicOutput> Fields { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<InputArea, InputAreaBasicOutput>()
                .ForMember(d => d.Fields, m => m.Ignore());
        }
    }

    public class InputAreaFieldBasicOutput
    {
        public int InputAreaId { get; set; }
        public int InputAreaFieldId { get; set; }
        public string FieldName { get; set; }
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public EnumDataType DataTypeId { get; set; }
        public int DataSize { get; set; }
        public EnumFormType FormTypeId { get; set; }
        public string DefaultValue { get; set; }
        public string RefTableCode { get; set; }
        public string RefTableField { get; set; }
        public string RefTableTitle { get; set; }
        public bool IsRequire { get; set; }
        public int? DecimalPlace { get; set; }
    }
}
