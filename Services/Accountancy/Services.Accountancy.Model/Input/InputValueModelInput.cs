﻿using System.Collections.Generic;

namespace VErp.Services.Accountancy.Model.Input
{
    public class InputValueInputModel
    {
        public InputValueInputModel()
        {
        }
        public ICollection<InputValueRowInputModel> Rows { get; set; }
    }

    public class InputValueRowInputModel
    {
        public int InputAreaId { get; set; }
        //public bool IsMultiRow { get; set; }
        public long? InputValueRowId { get; set; }
        public ICollection<InputValueModel> Values { get; set; }
    }


    public class InputValueModel
    {
        public int InputAreaFieldId { get; set; }
        public string Value { get; set; }
    }
}
