using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;
using VErp.Commons.Library;
using VErp.Commons.Enums.StandardEnum;
using Microsoft.EntityFrameworkCore;
using VErp.Infrastructure.EF.EFExtensions;
using System.Linq;
using VErp.Infrastructure.ServiceCore.Model;
using Microsoft.Data.SqlClient;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class MaterialCalcService : IMaterialCalcService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IAsyncRunnerService _asyncRunner;
        private readonly ICurrentContextService _currentContext;
        private readonly IProductHelperService _productHelperService;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IMapper _mapper;
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
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _asyncRunner = asyncRunner;
            _currentContext = currentContext;
            _productHelperService = productHelperService;
            _customGenCodeHelperService = customGenCodeHelperService;
            _mapper = mapper;
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

            return (paged.OrderBy(d=>d.RowNumber).ToList(), total);
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

            var entity = _mapper.Map<MaterialCalc>(req);
            await _purchaseOrderDBContext.MaterialCalc.AddAsync(entity);
            await _purchaseOrderDBContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.MaterialCalc, entity.MaterialCalcId, $"Thêm mới tính nhu cầu VT {req.MaterialCalcCode}", req.JsonSerialize());

            await ctx.ConfirmCode();

            return entity.MaterialCalcId;
        }


        public async Task<MaterialCalcModel> Info(long materialCalcId)
        {
            var entity = await GetEntityIncludes(materialCalcId);
            if (entity == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy bảng tính");

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
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy bảng tính");

            await Validate(materialCalcId, req);
            _purchaseOrderDBContext.MaterialCalcConsumptionGroup.RemoveRange(entity.MaterialCalcConsumptionGroup);

            _purchaseOrderDBContext.MaterialCalcProductOrder.RemoveRange(entity.MaterialCalcProduct.SelectMany(p => p.MaterialCalcProductOrder));

            _purchaseOrderDBContext.MaterialCalcProductDetail.RemoveRange(entity.MaterialCalcProduct.SelectMany(p => p.MaterialCalcProductDetail));

            _purchaseOrderDBContext.MaterialCalcProduct.RemoveRange(entity.MaterialCalcProduct);

            _purchaseOrderDBContext.MaterialCalcSummary.RemoveRange(entity.MaterialCalcSummary);

            _mapper.Map(req, entity);

            await _purchaseOrderDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.MaterialCalc, entity.MaterialCalcId, $"Cập nhật tính nhu cầu VT {req.MaterialCalcCode}", req.JsonSerialize());

            return true;
        }

        public async Task<bool> Delete(long materialCalcId)
        {
            var entity = await GetEntityIncludes(materialCalcId);
            if (entity == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy bảng tính");

            entity.IsDeleted = true;
            await _purchaseOrderDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.MaterialCalc, entity.MaterialCalcId, $"Xóa tính nhu cầu VT {entity.MaterialCalcCode}", new { materialCalcId }.JsonSerialize());

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
              .FirstOrDefaultAsync(c => c.MaterialCalcId == materialCalcId);
        }

        private async Task Validate(long? materialCalcId, MaterialCalcModel model)
        {
            if (materialCalcId > 0 && string.IsNullOrWhiteSpace(model.MaterialCalcCode))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Vui lòng nhập mã số");
            }
            model.MaterialCalcCode = (model.MaterialCalcCode ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(model.MaterialCalcCode))
            {
                if (await _purchaseOrderDBContext.MaterialCalc.AnyAsync(s => s.MaterialCalcId != materialCalcId && s.MaterialCalcCode == model.MaterialCalcCode))
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Mã số đã tồn tại");
                }
            }
        }

        private async Task<GenerateCodeContext> GenerateCode(long? materialCalcId, MaterialCalcModel model)
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
