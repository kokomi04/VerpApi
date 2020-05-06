using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Services.Accountant.Model.Category;

namespace VErp.Services.Accountant.Model.Input
{
    public class InputAreaFieldStyleInputModel : IMapFrom<InputAreaFieldStyle>
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string TitleStyleJson { get; set; }
        public string InputStyleJson { get; set; }
        public string OnFocus { get; set; }
        public string OnKeydown { get; set; }
        public string OnKeypress { get; set; }
        public string OnBlur { get; set; }
        public string OnChange { get; set; }
        public bool AutoFocus { get; set; }
        public int Column { get; set; }
    }

    public class InputAreaFieldStyleOutputModel : InputAreaFieldStyleInputModel
    {
        public int InputAreaFieldId { get; set; }
    }
}
