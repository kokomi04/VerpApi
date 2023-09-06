using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ReportConfigDB;

namespace Verp.Services.ReportConfig.Model
{
   
    public class ReportTypeCustomImportModel : IMapFrom<ReportTypeCustom>
    {
        public int ReportTypeId { get; set; }

        public string HeadSql { get; set; }

        public string BodySql { get; set; }

        public string FooterSql { get; set; }
    }
    public class ReportTypeCustomModel : ReportTypeCustomImportModel
    {
        public int SubsidiaryId { get; set; }

        public int IsDeleted { get; set; }

    }
}
