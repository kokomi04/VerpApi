using System;
using System.Collections.Generic;

namespace VErp.Services.Accountant.Model.Input
{

    public class InputValueOuputModel
    {
        public InputValueOuputModel()
        {
            Rows = new List<InputRowOutputModel>();
        }
        public ICollection<InputRowOutputModel> Rows { get; set; }
    }

    public class InputRowOutputModel
    {
        public InputRowOutputModel()
        {
            FieldValues = new Dictionary<int, string>();
        }
        public int InputAreaId { get; set; }
        public long InputValueRowId { get; set; }
        public IDictionary<int, string> FieldValues { get; set; }
    }
}
