using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.HrConfig
{
    public class HrTypeBasicOutput : IMapFrom<HrType>
    {
        public string Title { get; set; }
        public string HrTypeCode { get; set; }
        public IList<HrAreaBasicOutput> Areas { get; set; }
        // public IList<HrTypeViewModelList> Views { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<HrType, HrTypeBasicOutput>()
                .ForMember(d => d.Areas, m => m.Ignore());
                // .ForMember(d => d.Views, m => m.Ignore());
        }
    }

    public class HrAreaBasicOutput : IMapFrom<HrArea>
    {
        public int HrAreaId { get; set; }
        public string Title { get; set; }
        public string HrAreaCode { get; set; }
        public bool IsMultiRow { get; set; }
        public int Columns { get; set; }
        public string ColumnStyles { get; set; }
        public IList<HrAreaFieldBasicOutput> Fields { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<HrArea, HrAreaBasicOutput>()
                .ForMember(d => d.Fields, m => m.Ignore());
        }
    }

    public class HrAreaFieldBasicOutput
    {
        public int HrAreaId { get; set; }
        public int HrAreaFieldId { get; set; }
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

        public string ReferenceUrlExec { get; set; }
    }
}
