using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.Data
{
    public class DataRefModel
    {
        public EnumObjectType ObjectTypeId { get; set; }
        public int BillTypeId { get; set; }
        public string ObjectCode { get; set; }
        public long ObjectId { get; set; }
        public string ObjectTitle { get; set; }
        public EnumObjectType? Parent_ObjectTypeId { get; set; }
        public string Parent_ObjectCode { get; set; }
    }
}
