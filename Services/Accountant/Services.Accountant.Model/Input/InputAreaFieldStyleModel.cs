using System.ComponentModel.DataAnnotations;
using VErp.Services.Accountant.Model.Category;

namespace VErp.Services.Accountant.Model.Input
{
    public abstract class InputAreaFieldStyleModel
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

    public class InputAreaFieldStyleInputModel : InputAreaFieldStyleModel
    {
    }

    public class InputAreaFieldStyleOutputModel : InputAreaFieldStyleModel
    {
        public int InputAreaFieldId { get; set; }
    }
}
