using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class ActionButtonIdentity
    {
        public int ActionButtonId { get; set; }
        public EnumObjectType ObjectTypeId { get; set; }
        public int ObjectId { get; set; }
        //Addition
        public string ObjectTitle { get; set; }

    }
    public class ActionButtonSimpleModel : ActionButtonIdentity
    {
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


    }

    public class ActionButtonModel : ActionButtonSimpleModel
    {
        public string SqlAction { get; set; }
    }

    public class BillInfoModel
    {
        public NonCamelCaseDictionary Info { get; set; }
        public IList<NonCamelCaseDictionary> Rows { get; set; }
        public OutsideImportMappingData OutsideImportMappingData { get; set; }
    }

    public class OutsideImportMappingData
    {
        public string MappingFunctionKey { get; set; }
        public string ObjectId { get; set; }
    }
}
