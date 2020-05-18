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
        public int ClientObjectProcessStepId { get; set; }
        public int? ObjectProcessStepId { get; set; }
        public string ObjectProcessStepName { get; set; }
        public IList<int> DependClientObjectProcessStepIds { get; set; }
        public IList<int> UserIds { get; set; }
    }    
}
