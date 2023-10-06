using VErp.Commons.GlobalObject.InternalDataInterface.Organization;

namespace VErp.Services.Organization.Model.Department
{
    public class DepartmentModel : DepartmentSimpleModel
    {
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public bool IsActived { get; set; }
        public bool IsProduction { get; set; }
        public long? ImageFileId { get; set; }
        public bool IsFactory { get; set; }
    }
    public class DepartmentExtendModel : DepartmentModel
    {
        public string PathCodes { get; set; }
        public string PathNames { get; set; }
        public int? Level { get; set; } 
    }
}
