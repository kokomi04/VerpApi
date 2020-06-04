﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

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

    public class SubsidiaryOutput : SubsidiaryModel
    {
        public int SubsidiaryId { get; set; }

    }

}