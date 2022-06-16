using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DynamicBill;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.Input
{

    public class InputTypeGlobalSettingModel : ITypeData, IMapFrom<InputTypeGlobalSetting>
    {
        public InputTypeGlobalSettingModel()
        {
        }


        public string PreLoadAction { get; set; }
        public string PostLoadAction { get; set; }
        public string AfterLoadAction { get; set; }
        public string BeforeSubmitAction { get; set; }
        public string BeforeSaveAction { get; set; }
        public string AfterSaveAction { get; set; }
        public string AfterUpdateRowsJsAction { get; set; }

    }
}
