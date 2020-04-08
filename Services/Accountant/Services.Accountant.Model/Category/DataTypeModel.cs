
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Category

{
    public class DataTypeModel
    {
        public int DataTypeId { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public int DataSizeDefault { get; set; }

        public string RegularExpression { get; set; }
    }
}
