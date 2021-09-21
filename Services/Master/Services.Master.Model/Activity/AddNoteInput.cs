﻿using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.Activity
{
    public class AddNoteInput
    {
        public int? BillTypeId { get; set; }
        public EnumObjectType ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
        public string Message { get; set; }
    }
}
