
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Input

{
    public abstract class InputValueBillModel
    {
        public int InputTypeId { get; set; }
    }

    public class InputValueBillInputModel: InputValueBillModel
    {
        
        public InputValueBillInputModel()
        {
            InputValueRows = new HashSet<InputValueRowInputModel>();
        }
        public ICollection<InputValueRowInputModel> InputValueRows { get; set; }
    }

    public class InputValueBillOutputModel : InputValueBillModel
    {
        public long InputValueBillId { get; set; }
        public InputValueBillOutputModel()
        {
            InputValueRows = new HashSet<InputValueRowOutputModel>();
        }
        public ICollection<InputValueRowOutputModel> InputValueRows { get; set; }
    }
}
