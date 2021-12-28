using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Customer
{
    public class CustomerCateModel : IMapFrom<CustomerCate>
    {
        public int? CustomerCateId { get; set; }
        public string CustomerCateCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
    }
}
