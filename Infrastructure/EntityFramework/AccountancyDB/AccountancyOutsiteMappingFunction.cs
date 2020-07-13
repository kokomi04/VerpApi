using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class AccountancyOutsiteMappingFunction
    {
        public int AccountancyOutsiteMappingFunctionId { get; set; }
        public string MappingFunctionKey { get; set; }
        public string FunctionName { get; set; }
        public string Description { get; set; }
        public bool IsWarningOnDuplicated { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
    }
}
