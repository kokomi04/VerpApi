
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Input

{
    public abstract class InputValueRowModel
    {
        public InputValueRowModel()
        {
        }
        public int InputAreaId { get; set; }
    }

    public class InputValueRowInputModel: InputValueRowModel
    {
        public InputValueRowInputModel()
        {
            InputValueRowVersions = new HashSet<InputValueRowVersionInputModel>();
        }

        public ICollection<InputValueRowVersionInputModel> InputValueRowVersions { get; set; }
    }

    public class InputValueRowOutputModel : InputValueRowModel
    {
        public InputValueRowOutputModel()
        {
            InputValueRowVersions = new HashSet<InputValueRowVersionOutputModel>();
        }
        public long InputValueRowId { get; set; }
        public long InputValueBillId { get; set; }
        public long LastestInputValueRowVersionId { get; set; }
        public ICollection<InputValueRowVersionOutputModel> InputValueRowVersions { get; set; }
    }

}
