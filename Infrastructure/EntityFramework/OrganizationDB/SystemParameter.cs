using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class SystemParameter
    {
        public int SystemParameterId { get; set; }
        public string FieldName { get; set; }
        public string Name { get; set; }
        public int DataTypeId { get; set; }
        public string Value { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDateTimeUtc { get; set; }
        public DateTime UpdatedDateTimeUtc { get; set; }
        public bool IsDeleted { get; set; }
    }
}
