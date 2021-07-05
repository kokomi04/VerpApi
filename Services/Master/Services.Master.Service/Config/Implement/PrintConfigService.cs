using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.Attributes;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class PrintConfigService : IPrintConfigService
    {
        private readonly MasterDBContext _masterDBContext;

        public PrintConfigService(MasterDBContext accountancyDBContext)
        {
            _masterDBContext = accountancyDBContext;
        }

        public async Task<IList<EntityField>> GetSuggestionField(int moduleTypeId)
        {
            var parammeters = new SqlParameter[]
                {
                    new SqlParameter("@ModuleTypeId", moduleTypeId)
                };
            var resultData = await _masterDBContext.ExecuteDataProcedure("asp_PrintConfig_SuggestionField", parammeters);
            return resultData.ConvertData<EntityField>()
                .ToList();
        }

        public Task<IList<EntityField>> GetSuggestionField(Assembly assembly)
        {
            var classTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttributes().Any(a => a is PrintSuggestionConfigAttribute))
                .ToArray();
            IList<EntityField> fields = new List<EntityField>();
            foreach (var type in classTypes)
            {
                foreach (var prop in type.GetProperties())
                {
                    EntityField field = new EntityField
                    {
                        FieldName = prop.Name,
                        Title = prop.GetCustomAttributes<System.ComponentModel.DataAnnotations.DisplayAttribute>().FirstOrDefault()?.Name ?? prop.Name,
                        Group = prop.GetCustomAttributes<System.ComponentModel.DataAnnotations.DisplayAttribute>().FirstOrDefault()?.GroupName
                    };
                    fields.Add(field);
                }
            }

            return Task.FromResult(fields);
        }
    }
}

