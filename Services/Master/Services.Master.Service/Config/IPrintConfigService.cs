using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Master.Service.Config
{
    public interface IPrintConfigService
    {
        Task<IList<EntityField>> GetSuggestionField(int moduleTypeId);
        Task<IList<EntityField>> GetSuggestionField(Assembly assembly);
    }
}
