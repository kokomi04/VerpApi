using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Customer
{   

    public class CustomerNotifyPartyModel : IMapFrom<CustomerNotifyParty>
    {
        public long? CustomerNotifyPartyId { get; set; }
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public EnumCustomerNotifyPartyStatus CustomerNotifyPartyStatusId { get; set; }

    }
}
