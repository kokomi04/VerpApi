using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ReportConfigDB;

public partial class ReportTypeCustom
{
    public int ReportTypeCustomId { get; set; }

    public int ReportTypeId { get; set; }

    public int SubsidiaryId { get; set; }

    public bool IsDeleted { get; set; }

    public string HeadSql { get; set; }

    public string BodySql { get; set; }

    public string FooterSql { get; set; }
}
