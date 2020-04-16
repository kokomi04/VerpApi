
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Input

{
    public class InputValueBillInputModel
    {
        public InputValueBillInputModel()
        {
            Values = new HashSet<InputValueRowModel>();
        }
        public ICollection<InputValueRowModel> Values { get; set; }
    }

    public class InputValueBillOutputModel : InputValueBillInputModel
    {
        public long InputValueBillId { get; set; }
    }
}
