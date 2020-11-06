using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.Org;
using VErp.Infrastructure.EF.OrganizationDB;
using EmployeeEntity = VErp.Infrastructure.EF.OrganizationDB.Employee;

namespace Services.Organization.Model.Deparment
{
    public class SubsidiaryModel : IMapFrom<Subsidiary>
    {
        public int? ParentSubsidiaryId { get; set; }
        public string SubsidiaryCode { get; set; }
        public string SubsidiaryName { get; set; }
        public string Address { get; set; }
        public string TaxIdNo { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }
        public string Description { get; set; }
    }

    //public class SubsidiaryCreateModel : SubsidiaryModel
    //{
    //    public SubsidiaryOwnerCreateModel Owner { get; set; }
    //    public void Mapping(Profile profile) => profile.CreateMap<SubsidiaryCreateModel, Subsidiary>()
    //        .ReverseMap()
    //        .ForMember(m => m.Owner, v => v.Ignore());
    //}

    public class SubsidiaryOutput : SubsidiaryModel
    {
        public int SubsidiaryId { get; set; }
        public SubsidiaryOwnerModel Owner { get; set; }
    }

    public class SubsidiaryOwnerModel : EmployeeBase, IMapFrom<EmployeeEntity>
    {
        public int UserId { get; set; }
    }
}
