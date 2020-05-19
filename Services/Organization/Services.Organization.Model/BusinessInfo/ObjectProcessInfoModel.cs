using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace Services.Organization.Model.BusinessInfo
{
    public class ObjectProcessInfoModel
    {
        public EnumObjectProcessType ObjectProcessTypeId { get; set; }
        public string ObjectProcessTypeName { get; set; }
    }

    public class ObjectProcessInfoStepModel
    {
        public int SortOrder { get; set; }
        public string ObjectProcessStepName { get; set; }
        public IList<int> DependObjectProcessStepIds { get; set; }
        public IList<int> UserIds { get; set; }
    }
    public class ObjectProcessInfoStepListModel : ObjectProcessInfoStepModel
    {
        public int ObjectProcessStepId { get; set; }
    }
}
