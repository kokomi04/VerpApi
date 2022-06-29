using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Verp.Resources.Stock.Product;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Stock.Model.Product.Calc;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductPurityCalcService : IProductPurityCalcService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _productPurityCalcActivityLog;

        public ProductPurityCalcService(StockDBContext stockContext
            , ILogger<PropertyService> logger
            , IActivityLogService activityLogService
            , IMapper mapper)
        {
            _stockDbContext = stockContext;
            _logger = logger;
            _mapper = mapper;
            _productPurityCalcActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ProductPurityCalc);
        }

        public async Task<int> Create(ProductPurityCalcModel req)
        {
            var info = _mapper.Map<ProductPurityCalc>(req);

            await _stockDbContext.ProductPurityCalc.AddAsync(info);

            await _stockDbContext.SaveChangesAsync();

            await _productPurityCalcActivityLog.LogBuilder(() => ProductPurityCalcActivityLogMessage.Create)
                 .MessageResourceFormatDatas(info.Title)
                 .ObjectId(info.ProductPurityCalcId)
                 .JsonData(req.JsonSerialize())
                 .CreateLog();
            return info.ProductPurityCalcId;
        }

        public async Task<bool> Delete(int productPurityCalcId)
        {
            var info = await _stockDbContext.ProductPurityCalc.FirstOrDefaultAsync(c => c.ProductPurityCalcId == productPurityCalcId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            _stockDbContext.Remove(info);

            await _stockDbContext.SaveChangesAsync();

            await _productPurityCalcActivityLog.LogBuilder(() => ProductPurityCalcActivityLogMessage.Delete)
                 .MessageResourceFormatDatas(info.Title)
                 .ObjectId(info.ProductPurityCalcId)
                 .JsonData(info.JsonSerialize())
                 .CreateLog();
            return true;
        }

        public async Task<ProductPurityCalcModel> GetInfo(int productPurityCalcId)
        {
            var info = await _stockDbContext.ProductPurityCalc.FirstOrDefaultAsync(c => c.ProductPurityCalcId == productPurityCalcId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            return _mapper.Map<ProductPurityCalcModel>(info);
        }

        public async Task<IList<ProductPurityCalcModel>> GetList()
        {
            var lst = await _stockDbContext.ProductPurityCalc.ToListAsync();
            return _mapper.Map<List<ProductPurityCalcModel>>(lst);
        }

        public async Task<bool> Update(int productPurityCalcId, ProductPurityCalcModel req)
        {
            var info = await _stockDbContext.ProductPurityCalc.FirstOrDefaultAsync(c => c.ProductPurityCalcId == productPurityCalcId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            _mapper.Map(req, info);
            await _stockDbContext.SaveChangesAsync();

            await _productPurityCalcActivityLog.LogBuilder(() => ProductPurityCalcActivityLogMessage.Update)
                 .MessageResourceFormatDatas(info.Title)
                 .ObjectId(info.ProductPurityCalcId)
                 .JsonData(info.JsonSerialize())
                 .CreateLog();

            return true;
        }
    }
}
