using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.BusinessInfo
{
    public class ObjectApprovalStepModel: IMapFrom<ObjectApprovalStep>
    {
        public int ObjectApprovalStepId { get; set; }
        public int ObjectTypeId { get; set; }
        public int ObjectId { get; set; }
        public EnumObjectApprovalStepType ObjectApprovalStepTypeId { get; set; }
        public bool IsEnable { get; set; }
        public string ObjectFieldEnable { get; set; }
    }

    public class ObjectApprovalStepItemModel
    {
        public EnumModuleType ModuleTypeId { get; set; }
        public string ModuleTypeName { get; set; }
        public EnumObjectType ObjectTypeId { get; set; }
        public string ObjectTypeName { get; set; }
        public int ObjectId { get; set; }
        public string ObjectName { get; set; }
        public int? ObjectGroupId { get; set; }
    }


}