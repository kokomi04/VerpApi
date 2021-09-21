using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.Config
{
    public class I18nLanguageModel: IMapFrom<I18nLanguage>
    {
        public long I18nLanguageId { get; set; }
        public string Key { get; set; }
        public string Vi { get; set; }
        public string En { get; set; }
    }
}