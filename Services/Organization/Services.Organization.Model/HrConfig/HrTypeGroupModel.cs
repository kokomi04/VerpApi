using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.HrConfig
{
    public class HrTypeGroupModel : IMapFrom<HrTypeGroup>
    {
        public string HrTypeGroupName { get; set; }
        public int SortOrder { get; set; }
    }

    public class HrTypeGroupList : HrTypeGroupModel
    {
        public int HrTypeGroupId { get; set; }
    }
}