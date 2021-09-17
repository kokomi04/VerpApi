using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using CustomerEntity = VErp.Infrastructure.EF.OrganizationDB.Customer;

namespace VErp.Services.Organization.Model.Customer
{
    public class CustomerListBasicOutput : IMapFrom<CustomerEntity>
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
    }
    public class CustomerListOutput : CustomerListBasicOutput, IMapFrom<CustomerEntity>
    {

        public EnumCustomerType CustomerTypeId { get; set; }
        public string Address { get; set; }
        public string TaxIdNo { get; set; }
        public string PhoneNumber { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Identify { get; set; }
        public EnumCustomerStatus CustomerStatusId { get; set; }

        public int? DebtDays { get; set; }
        public decimal? DebtLimitation { get; set; }
        public EnumBeginningType DebtBeginningTypeId { get; set; }
        public int? DebtManagerUserId { get; set; }

        public int? LoanDays { get; set; }
        public decimal? LoanLimitation { get; set; }
        public EnumBeginningType LoanBeginningTypeId { get; set; }
        public int? LoanManagerUserId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CustomerEntity, CustomerListOutput>()
                .ForMember(d => d.CustomerTypeId, s => s.MapFrom(m => (EnumCustomerType)m.CustomerTypeId))
                .ForMember(d => d.CustomerStatusId, s => s.MapFrom(m => (EnumCustomerStatus)m.CustomerStatusId))
                .ForMember(d => d.DebtBeginningTypeId, s => s.MapFrom(m => (EnumBeginningType)m.DebtBeginningTypeId))
                .ForMember(d => d.LoanBeginningTypeId, s => s.MapFrom(m => (EnumBeginningType)m.LoanBeginningTypeId));
        }

    }

    public class CustomerListFilterModel
    {
        public string Keyword { get; set; }
        public IList<int> CustomerIds { get; set; }
        public EnumCustomerStatus? CustomerStatusId { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
    }


    public class CustomerListExportModel: CustomerListFilterModel
    {
        public IList<string> FieldNames { get; set; }
    }
}
