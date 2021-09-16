
using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DynamicBill;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;

namespace Services.Organization.Model.HrConfig
{
    public class HrTypeSimpleProjectMappingModel : HrTypeSimpleModel, IMapFrom<HrType>
    {

    }

    public class HrTypeModel : HrTypeSimpleProjectMappingModel, ITypeData
    {
        public HrTypeModel()
        {
        }


        public string PreLoadAction { get; set; }
        public string PostLoadAction { get; set; }
        public string AfterLoadAction { get; set; }
        public string BeforeSubmitAction { get; set; }
        public string BeforeSaveAction { get; set; }
        public string AfterSaveAction { get; set; }
        public string AfterUpdateRowsJsAction { get; set; }

        //public MenuStyleModel MenuStyle { get; set; }
    }

}
