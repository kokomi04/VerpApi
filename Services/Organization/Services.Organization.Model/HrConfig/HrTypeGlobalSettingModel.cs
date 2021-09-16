using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DynamicBill;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.HrConfig
{
    public class HrTypeGlobalSettingModel: ITypeData, IMapFrom<HrTypeGlobalSetting>
    {
        public HrTypeGlobalSettingModel()
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