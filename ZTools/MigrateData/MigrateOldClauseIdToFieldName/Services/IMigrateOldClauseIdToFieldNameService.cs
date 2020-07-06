using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Infrastructure.EF.EFExtensions;
using NewCategoryField = VErp.Infrastructure.EF.AccountancyDB.CategoryField;
using OldCategoryField = VErp.Infrastructure.EF.AccountingDB.CategoryField;

namespace MigrateOldClauseIdToFieldName.Services
{
    public interface IMigrateOldClauseIdToFieldNameService
    {
        Task Execute();
    }
    public class MigrateOldClauseIdToFieldNameService : IMigrateOldClauseIdToFieldNameService
    {
        private readonly AccountancyDBContext _accountancyDBContext;
        private readonly AccountingDBContext _accountingDBContext;

        public MigrateOldClauseIdToFieldNameService(AccountancyDBContext accountancyDBContext, AccountingDBContext accountingDBContext)
        {
            _accountancyDBContext = accountancyDBContext;
            _accountingDBContext = accountingDBContext;

        }

        public class CategoryFieldData
        {
            public int CategoryFieldId { get; set; }
            public string CategoryFieldName { get; set; }
            public int CategoryId { get; set; }
            public string CategoryCode { get; set; }
        }
        public async Task Execute()
        {
            var toUpdateCategoryFields = await _accountancyDBContext.CategoryField.ToListAsync();

            var newCategoryFields = await (from f in _accountancyDBContext.CategoryField
                                           join c in _accountancyDBContext.Category on f.CategoryId equals c.CategoryId
                                           select new CategoryFieldData
                                           {
                                               CategoryFieldId = f.CategoryFieldId,
                                               CategoryFieldName = f.CategoryFieldName,
                                               CategoryId = c.CategoryId,
                                               CategoryCode = c.CategoryCode
                                           }
                                           ).ToListAsync();

            var oldCategoryFields = await (from f in _accountingDBContext.CategoryField
                                           join c in _accountingDBContext.Category on f.CategoryId equals c.CategoryId
                                           select new CategoryFieldData
                                           {
                                               CategoryFieldId = f.CategoryFieldId,
                                               CategoryFieldName = f.CategoryFieldName,
                                               CategoryId = c.CategoryId,
                                               CategoryCode = c.CategoryCode
                                           }
                                           ).ToListAsync();

            var areaFields = await _accountancyDBContext.InputAreaField.ToListAsync();

            foreach (var areaField in areaFields)
            {
                var clause = areaField.Filters.JsonDeserialize<JToken>();
                if (clause != null)
                {
                    await NormalizeClauseProcess(clause, newCategoryFields, oldCategoryFields);
                }

                areaField.Filters = JsonConvert.SerializeObject(clause);
            }

            foreach (var categoryField in toUpdateCategoryFields)
            {
                var clause = categoryField.Filters.JsonDeserialize<JToken>();
                if (clause != null)
                {
                    await NormalizeClauseProcess(clause, newCategoryFields, oldCategoryFields);
                }
                categoryField.Filters = JsonConvert.SerializeObject(clause);
            }

            await _accountancyDBContext.SaveChangesAsync();
        }

        public async Task NormalizeClauseProcess(JToken clause, IList<CategoryFieldData> newCategoryFields, IList<CategoryFieldData> oldCategoryFields)
        {
            if (clause != null)
            {
                if (!(clause is JObject))
                {
                    return;
                }
                var props = (clause as JObject).Properties();
                bool isSingle = props.Any(c => c.Name.ToLower() == nameof(SingleClause.Operator).ToLower());
                bool isArray = props.Any(c => c.Name.ToLower() == nameof(ArrayClause.Condition).ToLower());

                if (isSingle)
                {
                    var oldCategoryField = oldCategoryFields.FirstOrDefault(f => f.CategoryFieldId == clause["field"].Value<int>());

                    var newCategoryField = newCategoryFields.FirstOrDefault(f => f.CategoryCode == "_" + oldCategoryField.CategoryCode && f.CategoryFieldName == oldCategoryField.CategoryFieldName);

                    clause["id"] = newCategoryField.CategoryFieldId;
                    clause["field"] = newCategoryField.CategoryFieldId;
                    clause["fieldName"] = newCategoryField.CategoryFieldName;
                }
                else if (isArray)
                {
                    var rules = (clause["rules"] as JArray);
                    for (int indx = 0; indx < rules.Count; indx++)
                    {
                        await NormalizeClauseProcess(rules[indx], newCategoryFields, oldCategoryFields);
                    }
                }
            }
        }
    }
}
