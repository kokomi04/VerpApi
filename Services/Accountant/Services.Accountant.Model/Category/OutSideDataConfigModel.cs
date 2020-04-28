
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Category
{
    public class OutSideDataConfigModel
    {
        public int ModuleType { get; set; }
        public string Url { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
    }
}
