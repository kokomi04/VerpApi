using AutoMapper;
using NPOI.HPSF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.Report;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ReportConfigDB;

namespace Verp.Services.ReportConfig.Model
{
    public class ReportTypeListModel : IMapFrom<ReportType>
    {
        public int? ReportTypeId { get; set; }
        public int ReportTypeGroupId { get; set; }
        public string ReportTypeName { get; set; }
        public int SortOrder { get; set; }
    }

    public class ReportTypeModel : ReportTypeListModel
    {
        public string ReportPath { get; set; }

        public string MainView { get; set; }
        public string Joins { get; set; }
        public string Wheres { get; set; }
        public string OrderBy { get; set; }

        public string Head { get; set; }
        public string Footer { get; set; }
        public string HeadSql { get; set; }
        public string BodySql { get; set; }
        public string FooterSql { get; set; }
        public string PrintTitle { get; set; }
        public string GroupColumns { get; set; }
        public string Sign { get; set; }
        public string HtmlTemplate { get; set; }
       
        public EnumReportDetailOpenType? DetailOpenTypeId { get; set; }
        public EnumReportDetailTarget? DetailTargetId { get; set; }
        public int? DetailReportId { get; set; }
        public string DetailReportParams { get; set; }

        public string OnLoadJsCode { get; set; }
        public string PreLoadDataJsCode { get; set; }
        public string AfterLoadDataJsCode { get; set; }
        public string OnCloseJsCode { get; set; }
        public string OnCellClickJsCode { get; set; }
        public string OnCellChangeValueJsCode { get; set; }
        public string HeadPrint { get; set; }
        public long? TemplateFileId { get; set; }
        public string GroupTitleSql { get; set; }

        public IList<ReportColumnModel> Columns { get; set; }
        public bool IsBsc { get; set; }
        public BscConfigModel BscConfig { get; set; }

        public List<ReportColumnModel> ParseColumns(string column)
        {
            return column.JsonDeserialize<List<ReportColumnModel>>()?.OrderBy(c => c.SortOrder)?.ToList();
        }

        public int ReportModuleTypeId { get; set; }

        public void Mapping(Profile profile) => profile.CreateMap<ReportType, ReportTypeModel>()
       .ForMember(m => m.Columns, m => m.MapFrom(v => ParseColumns(v.Columns)))
       .ForMember(m => m.BscConfig, m => m.MapFrom(v => v.BscConfig.JsonDeserialize<BscConfigModel>()))
       .ForMember(m => m.ReportModuleTypeId, m => m.MapFrom(v => v.ReportTypeGroup.ModuleTypeId))
       .ReverseMap()
       .ForMember(m => m.Columns, m => m.MapFrom(v => v.Columns.JsonSerialize()))
       .ForMember(m => m.BscConfig, m => m.MapFrom(v => v.BscConfig.JsonSerialize()))
       .ForMember(m => m.ReportTypeGroup, m => m.Ignore());

    }

}
