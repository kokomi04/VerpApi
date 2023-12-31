﻿using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill
{
    public class ActionButtonBillTypeMappingModel
    {
        public int ActionButtonId { get; set; }
    }

    public class ActionButtonIdentity
    {
        public int ActionButtonId { get; set; }
        public EnumObjectType BillTypeObjectTypeId { get; set; }
    }

    public class ActionButtonBillTypeMapping
    {
        public int ActionButtonId { get; set; }
        public EnumObjectType BillTypeObjectTypeId { get; set; }
        public long BillTypeObjectId { get; set; }
    }


    public class ActionButtonActionType
    {
        public int ActionButtonId { get; set; }
        public EnumObjectType BillTypeObjectTypeId { get; set; }
        public long BillTypeObjectId { get; set; }

        public int ActionType { get; set; }
    }

    public class ActionButtonBaseModel
    {
        public int ActionButtonId { get; set; }

        public string ActionButtonCode { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên chức năng")]
        [MaxLength(256, ErrorMessage = "Tên chức năng quá dài")]
        public string Title { get; set; }
        public int SortOrder { get; set; }
        public string JsAction { get; set; }
        public string IconName { get; set; }
        public string Style { get; set; }
        public string JsVisible { get; set; }
        public int ActionTypeId { get; set; }
        public EnumActionPosition ActionPositionId { get; set; }
        public string SqlAction { get; set; }

    }


    public class ActionButtonModel : ActionButtonBaseModel
    {
        public EnumObjectType BillTypeObjectTypeId { get; set; }
    }

    public class ActionButtonUpdateModel : ActionButtonBaseModel, IMapFrom<ActionButtonModel>
    {


    }


}
