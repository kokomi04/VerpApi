using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Service.Inventory.Implement.Abstract
{
    public abstract class PuConversionValidateAbstract
    {
        public PuConversionValidateAbstract()
        {

        }

        protected readonly StockDBContext _stockDbContext;
        protected PuConversionValidateAbstract(StockDBContext stockDbContext)
        {
            _stockDbContext = stockDbContext;
        }

        protected async Task PuRateChangeValidateExistingInventoryData(IList<long> puIds)
        {
            var invDetail = await _stockDbContext.InventoryDetail.Where(d => puIds.Contains(d.ProductUnitConversionId)).FirstOrDefaultAsync();
            if (invDetail != null)
            {
                var info = await (from p in _stockDbContext.Product
                           join pu in _stockDbContext.ProductUnitConversion on p.ProductId equals pu.ProductId
                           where pu.ProductUnitConversionId == invDetail.ProductUnitConversionId
                           select new
                           {
                               p.ProductId,
                               p.ProductCode,
                               p.ProductName,
                               pu.ProductUnitConversionId,
                               pu.ProductUnitConversionName
                           }).FirstOrDefaultAsync();
                throw ProductErrorCode.SomeProductUnitConversionInUsed.BadRequest($"Không thể thay đổi tỷ lệ của đơn vị {info?.ProductUnitConversionName} mặt hàng {info?.ProductCode} do đã có phát sinh dữ liệu xuất/nhập kho! cần xóa hoặc thay thế bằng đơn vị khác!");
            }
        }
    }
}
