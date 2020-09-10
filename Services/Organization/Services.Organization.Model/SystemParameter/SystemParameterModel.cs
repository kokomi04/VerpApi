using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;

namespace Services.Organization.Model.SystemParameter
{
    public class SystemParameterModel
    {
        public int SystemParameterId { get; set; }
        public string Fieldname { get; set; }
        public string Name { get; set; }
        public EnumDataType DateTypeId { get; set; }
        public string Value { get; set; }
    }
}
