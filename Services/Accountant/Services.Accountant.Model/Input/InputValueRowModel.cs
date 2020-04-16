
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Input

{
    public class InputValueRowModel
    {
        public long InputValueRowId { get; set; }
        public long InputValueBillId { get; set; }
        public long LastestInputValueRowVersionId { get; set; }
        public int InputAreaId { get; set; }

        public InputValueRowVersionModel LastestInputValueRowVersion { get; set; }
    }
}
