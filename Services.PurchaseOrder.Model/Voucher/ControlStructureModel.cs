using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.PurchaseOrder.Model.Voucher
{
    public class ControlStructureModel
    {
        public string ControlTitle { get; set; }
        public IList<AreaStructureModel> Areas { get; set; }
        public IList<ControlButtonModel> Buttons { get; set; }
        public ControlStructureModel()
        {
            Areas = new List<AreaStructureModel>();
            Buttons = new List<ControlButtonModel>();
        }
    }

    public class ControlButtonModel 
    {
        public string Title { get; set; }
        public string ButtonCode { get; set; }
        public int SortOrder { get; set; }
        public string JsAction { get; set; }
        public string IconName { get; set; }
    }

    public class AreaStructureModel
    {
        public string AreaTitle { get; set; }
        public string AreaCode { get; set; }
        public bool IsMultiRow { get; set; }
        public int Columns { get; set; }
        public string SortOrder { get; set; }
        public IList<FieldStructureModel> Fields { get; set; }
        public AreaStructureModel()
        {
            Fields = new List<FieldStructureModel>();
        }
    }

    public class FieldStructureModel
    {
        public string FieldTitle { get; set; }
        public string FieldCode { get; set; }
        public string Column { get; set; }
        public string SortOrder { get; set; }
        public EnumFormType FormTypeId { get; set; }
        public EnumDataType DataTypeId { get; set; }
        public bool IsRequired { get; set; }
    }
}
