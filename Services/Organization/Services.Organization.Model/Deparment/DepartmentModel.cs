using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Organization.Model.Department
{
    public class DepartmentModel: DepartmentSimpleModel
    {
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public bool IsActived { get; set; }
        public bool IsProduction { get; set; }
        public decimal? WorkingHoursPerDay { get; set; }
    }
}
