using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Services.Accountancy.Model.Category
{
    public class CategoryFilterModel
    {
        public string Keyword { get; set; }
        public string Filters { get; set; }
        public string ExtraFilter { get; set; }

        public int Page { get; set; }
        public int Size { get; set; }
    }
}
