using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.PrintConfig
{
    public class PrintConfigHeaderCustomModel : PrintConfigHeaderCustomBaseModel
    {
        public string PrintConfigHeaderCustomCode { get; set; }

        public string JsAction { get; set; }
    }
    public class PrintConfigHeaderCustomViewModel : PrintConfigHeaderCustomBaseModel
    {
        public int PrintConfigHeaderCustomId { get; set; }
    }
    public class PrintConfigHeaderCustomBaseModel : IMapFrom<PrintConfigHeaderCustom>
    {
        public int? PrintConfigHeaderStandardId { get; set; }

        public string Title { get; set; }

        public bool? IsShow { get; set; }

        public int SortOrder { get; set; }
    }

    public class PrintConfigHeaderRollbackModel : PrintConfigHeaderCustom, IMapFrom<PrintConfigHeaderStandard>
    {
        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<PrintConfigHeaderStandard, PrintConfigHeaderRollbackModel>()
            .ForMember(m => m.PrintConfigHeaderCustomCode, v => v.MapFrom(m => m.PrintConfigHeaderStandardCode))
            .ReverseMapCustom();
        }
    }
}
