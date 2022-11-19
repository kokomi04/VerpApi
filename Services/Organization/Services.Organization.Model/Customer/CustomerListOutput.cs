using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using CustomerEntity = VErp.Infrastructure.EF.OrganizationDB.Customer;

namespace VErp.Services.Organization.Model.Customer
{
    public class CustomerListBasicOutput : BasicCustomerListModel, IMapFrom<CustomerEntity>
    {
     
    }

    public class CustomerListOutput : CustomerListModel, IMapFrom<CustomerEntity>
    {

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<CustomerEntity, CustomerListOutput>()
                .ForMember(d => d.CustomerTypeId, s => s.MapFrom(m => (EnumCustomerType)m.CustomerTypeId))
                .ForMember(d => d.CustomerStatusId, s => s.MapFrom(m => (EnumCustomerStatus)m.CustomerStatusId))
                .ForMember(d => d.DebtBeginningTypeId, s => s.MapFrom(m => (EnumBeginningType)m.DebtBeginningTypeId))
                .ForMember(d => d.LoanBeginningTypeId, s => s.MapFrom(m => (EnumBeginningType)m.LoanBeginningTypeId));
        }

    }

    public class CustomerListFilterModel
    {
        public string Keyword { get; set; }
        public int? CustomerCateId { get; set; }
        public IList<int> CustomerIds { get; set; }
        public EnumCustomerStatus? CustomerStatusId { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
    }


    public class CustomerListExportModel : CustomerListFilterModel
    {
        public IList<string> FieldNames { get; set; }
    }
}
