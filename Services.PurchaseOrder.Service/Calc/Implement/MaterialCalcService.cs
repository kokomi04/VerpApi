using AutoMapper;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.PurchaseOrder.Calc.MaterialCalc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;
using static Verp.Resources.PurchaseOrder.Calc.MaterialCalc.MaterialCalcValidationMessage;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class MaterialCalcService : IMaterialCalcService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IMapper _mapper;

        private readonly ObjectActivityLogFacade _materialCalcActivityLog;

        public MaterialCalcService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , IOptions<AppSetting> appSetting
           , ILogger<PurchasingSuggestService> logger
           , IActivityLogService activityLogService
           , IAsyncRunnerService asyncRunner
           , ICurrentContextService currentContext
           , IProductHelperService productHelperService
           , ICustomGenCodeHelperService customGenCodeHelperService
            , IMapper mapper
           )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _customGenCodeHelperService = customGenCodeHelperService;
            _mapper = mapper;

            _materialCalcActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.MaterialCalc);
        }

        public async Task<PageData<MaterialCalcListModel>> GetList(string keyword, ArrayClause filter, int page, int size)
        {
            keyword = keyword?.Trim();

            var sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@Keyword", $"%{keyword}%"));

            var rawSql = new StringBuilder($@"
    SELECT
         c.MaterialCalcId,
         c.MaterialCalcCode,
         c.Title,
         c.CreatedByUserId,
         c.CreatedDatetimeUtc,
         p.ProductId,
         p.ProductCode,
         p.ProductName,
         o.TotalOrderProductQuantity,
         o.OrderCodes,
         r.PurchasingRequestId,
         r.PurchasingRequestCode
    FROM dbo.MaterialCalc c
        JOIN dbo.MaterialCalcProduct d ON c.MaterialCalcId = d.MaterialCalcId
        JOIN dbo.RefProduct p on d.ProductId = p.ProductId
        LEFT JOIN(
            SELECT MaterialCalcProductId, STRING_AGG(OrderCode,',') OrderCodes, SUM(OrderProductQuantity) TotalOrderProductQuantity FROM dbo.MaterialCalcProductOrder GROUP BY MaterialCalcProductId
        ) o ON d.MaterialCalcProductId = o.MaterialCalcProductId
        LEFT JOIN dbo.PurchasingRequest r  on c.MaterialCalcId = r.MaterialCalcId
    WHERE c.IsDeleted=0 AND c.SubsidiaryId = @SubId
");
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                rawSql.AppendLine("AND");
                rawSql.AppendLine("(");
                rawSql.AppendLine("c.MaterialCalcCode LIKE @keyword");
                rawSql.AppendLine("OR c.Title LIKE @keyword");
                rawSql.AppendLine("OR p.ProductCode LIKE @keyword");
                rawSql.AppendLine("OR p.ProductName LIKE @keyword");
                rawSql.AppendLine("OR o.OrderCodes LIKE @keyword");
                rawSql.AppendLine(")");
            }


            rawSql.Insert(0, "FROM (\n");
            rawSql.AppendLine(") v");


            var suffix = 0;
            var whereCondition = new StringBuilder();

            if (filter != null && filter.Rules.Count > 0)
                filter.FilterClauseProcess("v", "v", ref whereCondition, ref sqlParams, ref suffix);
            if (whereCondition.Length > 0)
            {
                rawSql.AppendLine("WHERE");
                rawSql.AppendLine(whereCondition.ToString());
            }


            var totalTable = await _purchaseOrderDBContext.QueryDataTable("SELECT COUNT(DISTINCT MaterialCalcId) Total " + rawSql.ToString(), sqlParams.CloneSqlParams());
            int total = totalTable.Rows.Count > 0 ? (int)totalTable.Rows[0]["Total"] : 0;

            var listSql = $@";WITH tmp AS (
SELECT
                    DENSE_RANK () OVER (ORDER BY CreatedDatetimeUtc DESC,  MaterialCalcCode) RowNumber,
                    v.* {rawSql}
)
SELECT * FROM tmp WHERE RowNumber BETWEEN {(page - 1) * size + 1} AND {page * size}
";
            var paged = await _purchaseOrderDBContext.QueryList<MaterialCalcListModel>(listSql, sqlParams.CloneSqlParams());

            return (paged.OrderBy(d => d.RowNumber).ToList(), total);
        }

        public async IAsyncEnumerable<MaterialOrderProductHistory> GetHistoryProductOrderList(IList<int> productIds, IList<string> orderCodes)
        {
            var lst = await (
                from c in _purchaseOrderDBContext.MaterialCalc
                join d in _purchaseOrderDBContext.MaterialCalcProduct on c.MaterialCalcId equals d.MaterialCalcId
                join o in _purchaseOrderDBContext.MaterialCalcProductOrder on d.MaterialCalcProductId equals o.MaterialCalcProductId
                where productIds.Contains(d.ProductId) && orderCodes.Contains(o.OrderCode)
                select new
                {
                    c.MaterialCalcId,
                    c.MaterialCalcCode,
                    c.Title,
                    d.ProductId,
                    o.OrderCode,
                    o.OrderProductQuantity
                }).ToListAsync();

            var materialCalcIds = lst.Select(c => c.MaterialCalcId).ToList();

            var groups = await _purchaseOrderDBContext.MaterialCalcConsumptionGroup.Where(g => materialCalcIds.Contains(g.MaterialCalcId)).ToListAsync();

            foreach (var item in lst)
            {
                yield return new MaterialOrderProductHistory()
                {
                    MaterialCalcId = item.MaterialCalcId,
                    MaterialCalcCode = item.MaterialCalcCode,
                    Title = item.Title,
                    ConsumptionGroups = _mapper.Map<List<MaterialCalcConsumptionGroupModel>>(groups.Where(g => g.MaterialCalcId == item.MaterialCalcId)),

                    OrderCode = item.OrderCode,
                    ProductId = item.ProductId,
                    OrderProductQuantity = item.OrderProductQuantity,
                };
            }

        }


        public async Task<long> Create(MaterialCalcModel req)
        {
            var ctx = await GenerateCode(null, req);
            await Validate(null, req);

            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var entity = _mapper.Map<MaterialCalc>(req);
                //entity.MaterialCalcSummary = null;
                await _purchaseOrderDBContext.MaterialCalc.AddAsync(entity);
                await _purchaseOrderDBContext.SaveChangesAsync();

                //var subSumaryModels = new Dictionary<MaterialCalcSummarySubCalculation, MaterialCalcSummary>();

                //if (req.Summary != null)
                //{
                //    foreach (var sModel in req.Summary)
                //    {
                //        var sEntity = _mapper.Map<MaterialCalcSummary>(sModel);
                //        sEntity.MaterialCalcId = entity.MaterialCalcId;

                //        if (sModel.SubCalculations != null)
                //        {
                //            foreach (var subModel in sModel.SubCalculations)
                //            {
                //                var subEntity = _mapper.Map<MaterialCalcSummarySubCalculation>(subModel);
                //                subSumaryModels.Add(subEntity, sEntity);
                //            }
                //        }

                //    }
                //}


                //await _purchaseOrderDBContext.MaterialCalcSummary.AddRangeAsync(subSumaryModels.Values.Distinct());
                //await _purchaseOrderDBContext.SaveChangesAsync();
                //foreach(var (subEntity, sEntity) in subSumaryModels)
                //{
                //    subEntity.MaterialCalcSummaryId = sEntity.MaterialCalcSummaryId;
                //}

                //await _purchaseOrderDBContext.MaterialCalcSummarySubCalculation.AddRangeAsync(subSumaryModels.Keys);
                //await _purchaseOrderDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await ctx.ConfirmCode();


                await _materialCalcActivityLog.LogBuilder(() => MaterialCalcActivityLogMessage.Create)
                    .MessageResourceFormatDatas(entity.MaterialCalcCode)
                    .ObjectId(entity.MaterialCalcId)
                    .JsonData(req.JsonSerialize())
                    .CreateLog();

                return entity.MaterialCalcId;
            }

        }


        public async Task<MaterialCalcModel> Info(long materialCalcId)
        {
            var entity = await GetEntityIncludes(materialCalcId);
            if (entity == null)
                throw MaterialCalcNotFound.BadRequest();

            var requestInfo = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(r => r.MaterialCalcId == materialCalcId);

            var info = _mapper.Map<MaterialCalcModel>(entity);
            info.PurchasingRequestId = requestInfo?.PurchasingRequestId;
            info.PurchasingRequestCode = requestInfo?.PurchasingRequestCode;
            return info;
        }

        public async Task<bool> Update(long materialCalcId, MaterialCalcModel req)
        {
            var entity = await GetEntityIncludes(materialCalcId);
            if (entity == null)
                throw MaterialCalcNotFound.BadRequest();
            if (req.UpdatedDatetimeUtc != entity.UpdatedDatetimeUtc.GetUnix())
            {
                throw GeneralCode.DataIsOld.BadRequest();
            }

            await Validate(materialCalcId, req);
            _purchaseOrderDBContext.MaterialCalcConsumptionGroup.RemoveRange(entity.MaterialCalcConsumptionGroup);

            _purchaseOrderDBContext.MaterialCalcProductOrder.RemoveRange(entity.MaterialCalcProduct.SelectMany(p => p.MaterialCalcProductOrder));

            _purchaseOrderDBContext.MaterialCalcProductDetail.RemoveRange(entity.MaterialCalcProduct.SelectMany(p => p.MaterialCalcProductDetail));

            _purchaseOrderDBContext.MaterialCalcProduct.RemoveRange(entity.MaterialCalcProduct);

            _purchaseOrderDBContext.MaterialCalcSummarySubCalculation.RemoveRange(entity.MaterialCalcSummary.SelectMany(s=>s.MaterialCalcSummarySubCalculation));

            _purchaseOrderDBContext.MaterialCalcSummary.RemoveRange(entity.MaterialCalcSummary);

            _mapper.Map(req, entity);

            if (_purchaseOrderDBContext.HasChanges())
                entity.UpdatedDatetimeUtc = DateTime.UtcNow;

            await _purchaseOrderDBContext.SaveChangesAsync();

            await _materialCalcActivityLog.LogBuilder(() => MaterialCalcActivityLogMessage.Update)
                .MessageResourceFormatDatas(entity.MaterialCalcCode)
                .ObjectId(entity.MaterialCalcId)
                .JsonData(req.JsonSerialize())
                .CreateLog();

            return true;
        }

        public async Task<bool> Delete(long materialCalcId)
        {
            var entity = await GetEntityIncludes(materialCalcId);
            if (entity == null)
                throw MaterialCalcNotFound.BadRequest();

            entity.IsDeleted = true;
            await _purchaseOrderDBContext.SaveChangesAsync();

            await _materialCalcActivityLog.LogBuilder(() => MaterialCalcActivityLogMessage.Delete)
               .MessageResourceFormatDatas(entity.MaterialCalcCode)
               .ObjectId(entity.MaterialCalcId)
               .JsonData(entity.JsonSerialize())
               .CreateLog();

            return true;
        }

        private Task<MaterialCalc> GetEntityIncludes(long materialCalcId)
        {
            return _purchaseOrderDBContext.MaterialCalc
              .Include(c => c.MaterialCalcConsumptionGroup)
              .Include(c => c.MaterialCalcProduct)
              .ThenInclude(s => s.MaterialCalcProductDetail)
              .Include(s => s.MaterialCalcProduct)
              .ThenInclude(s => s.MaterialCalcProductOrder)
              .Include(s => s.MaterialCalcSummary)
              .ThenInclude(s=>s.MaterialCalcSummarySubCalculation)
              .FirstOrDefaultAsync(c => c.MaterialCalcId == materialCalcId);
        }

        private async Task Validate(long? materialCalcId, MaterialCalcModel model)
        {
            if (materialCalcId > 0 && string.IsNullOrWhiteSpace(model.MaterialCalcCode))
            {
                throw MaterialCalcCodeEmpty.BadRequest();
            }
            model.MaterialCalcCode = (model.MaterialCalcCode ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(model.MaterialCalcCode))
            {
                if (await _purchaseOrderDBContext.MaterialCalc.AnyAsync(s => s.MaterialCalcId != materialCalcId && s.MaterialCalcCode == model.MaterialCalcCode))
                {
                    throw MaterialCalcCodeAlreadyExist.BadRequest();
                }
            }
        }

        private async Task<IGenerateCodeContext> GenerateCode(long? materialCalcId, MaterialCalcModel model)
        {
            model.MaterialCalcCode = (model.MaterialCalcCode ?? "").Trim();

            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var code = await ctx
                .SetConfig(EnumObjectType.MaterialCalc)
                .SetConfigData(materialCalcId ?? 0, DateTime.Now.GetUnix())
                .TryValidateAndGenerateCode(_purchaseOrderDBContext.MaterialCalc, model.MaterialCalcCode, (s, code) => s.MaterialCalcId != materialCalcId && s.MaterialCalcCode == code);

            model.MaterialCalcCode = code;

            return ctx;
        }
    }
}
