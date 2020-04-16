
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Input

{
    public class InputValueRowModel
    {
        public InputValueRowModel()
        {
            InputValueRowVersions = new HashSet<InputValueRowVersionModel>();
        }
        public long InputValueRowId { get; set; }
        public long InputValueBillId { get; set; }
        public long LastestInputValueRowVersionId { get; set; }
        public int InputAreaId { get; set; }

        public ICollection<InputValueRowVersionModel> InputValueRowVersions { get; set; }
    }
}
