
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Input

{
    public class InputValueBillModel
    {
        public InputValueBillModel()
        {
            Values = new HashSet<InputValueRowModel>();
        }
        public ICollection<InputValueRowModel> Values { get; set; }
    }

    public class InputValueBillOutputModel : InputValueBillModel
    {
        public long InputValueBillId { get; set; }
    }
}
