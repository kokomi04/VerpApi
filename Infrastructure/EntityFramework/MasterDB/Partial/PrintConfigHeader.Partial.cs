using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.EF.MasterDB
{
    public interface IPrintConfigHeaderEntity
    {
        string Title { get; set; }

        string JsAction { get; set; }

        bool IsShow { get; set; }

        int SortOrder { get; set; }

        int CreatedByUserId { get; set; }

        DateTime CreatedDatetimeUtc { get; set; }

        int UpdatedByUserId { get; set; }

        DateTime UpdatedDatetimeUtc { get; set; }

        bool IsDeleted { get; set; }

        DateTime? DeletedDatetimeUtc { get; set; }
    }

    public partial class PrintConfigHeaderStandard : IPrintConfigHeaderEntity
    {

    }

    public partial class PrintConfigHeaderCustom : IPrintConfigHeaderEntity
    {

    }
}
