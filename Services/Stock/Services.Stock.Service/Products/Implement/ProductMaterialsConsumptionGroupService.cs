using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.Resources.Product;
using static VErp.Services.Stock.Service.Resources.Product.ConsumptionGroupValidationMessage;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductMaterialsConsumptionGroupService : IProductMaterialsConsumptionGroupService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _consumptionGroupActivityLog;

        public ProductMaterialsConsumptionGroupService(StockDBContext stockContext
            , IActivityLogService activityLogService
            , IMapper mapper)
        {
            _stockDbContext = stockContext;
            _mapper = mapper;
            _consumptionGroupActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ConsumptionGroup);
        }

        public async Task<int> AddProductMaterialsConsumptionGroup(ProductMaterialsConsumptionGroupModel model)
        {
            var group = _stockDbContext.ProductMaterialsConsumptionGroup.AsNoTracking().FirstOrDefault(x => x.ProductMaterialsConsumptionGroupCode == model.ProductMaterialsConsumptionGroupCode);
            if (group != null)
                throw CodeAlreadyExisted.BadRequestFormat(model.ProductMaterialsConsumptionGroupCode);

            var entity = _mapper.Map<ProductMaterialsConsumptionGroup>(model);
            _stockDbContext.ProductMaterialsConsumptionGroup.Add(entity);
            await _stockDbContext.SaveChangesAsync();

            await _consumptionGroupActivityLog.LogBuilder(() => ConsumptionGroupActivityLogMessage.Create)
               .MessageResourceFormatDatas(model.ProductMaterialsConsumptionGroupCode)
               .ObjectId(group.ProductMaterialsConsumptionGroupId)
               .JsonData(model.JsonSerialize())
               .CreateLog();

            return entity.ProductMaterialsConsumptionGroupId;
        }

        public async Task<bool> UpdateProductMaterialsConsumptionGroup(int groupId, ProductMaterialsConsumptionGroupModel model)
        {
            var group = _stockDbContext.ProductMaterialsConsumptionGroup.FirstOrDefault(x => x.ProductMaterialsConsumptionGroupId == groupId);
            if (group == null)
                throw ConsumptionGroupNotFound.BadRequest();

            if (group.ProductMaterialsConsumptionGroupCode != model.ProductMaterialsConsumptionGroupCode)
            {
                var check = _stockDbContext.ProductMaterialsConsumptionGroup.AsNoTracking()
                    .FirstOrDefault(x => x.ProductMaterialsConsumptionGroupCode == model.ProductMaterialsConsumptionGroupCode
                        && groupId != x.ProductMaterialsConsumptionGroupId);
                if (check != null)
                    throw CodeAlreadyExisted.BadRequestFormat(model.ProductMaterialsConsumptionGroupCode);
            }

            _mapper.Map(model, group);
            await _stockDbContext.SaveChangesAsync();

            await _consumptionGroupActivityLog.LogBuilder(() => ConsumptionGroupActivityLogMessage.Update)
                .MessageResourceFormatDatas(model.ProductMaterialsConsumptionGroupCode)
                .ObjectId(group.ProductMaterialsConsumptionGroupId)
                .JsonData(model.JsonSerialize())
                .CreateLog();
            return true;
        }

        public async Task<bool> DeleteProductMaterialsConsumptionGroup(int groupId)
        {
            var group = _stockDbContext.ProductMaterialsConsumptionGroup.FirstOrDefault(x => x.ProductMaterialsConsumptionGroupId == groupId);
            if (group == null)
                throw ConsumptionGroupNotFound.BadRequest();

            var hasGroupUsed = _stockDbContext.ProductMaterialsConsumption.AsNoTracking().Any(x => x.ProductMaterialsConsumptionGroupId == groupId);
            if (hasGroupUsed)
                throw CanNotDeleteConsumptionGroupInUsed.BadRequest();

            group.IsDeleted = true;
            await _stockDbContext.SaveChangesAsync();

            await _consumptionGroupActivityLog.LogBuilder(() => ConsumptionGroupActivityLogMessage.Delete)
              .MessageResourceFormatDatas(group.ProductMaterialsConsumptionGroupCode)
              .ObjectId(group.ProductMaterialsConsumptionGroupId)
              .JsonData(group.JsonSerialize())
              .CreateLog();
            return true;
        }

        public async Task<ProductMaterialsConsumptionGroupModel> GetProductMaterialsConsumptionGroup(int groupId)
        {
            var group = await _stockDbContext.ProductMaterialsConsumptionGroup.AsNoTracking().FirstOrDefaultAsync(x => x.ProductMaterialsConsumptionGroupId == groupId);
            if (group == null)
                throw ConsumptionGroupNotFound.BadRequest();

            return _mapper.Map<ProductMaterialsConsumptionGroupModel>(group);
        }

        public async Task<PageData<ProductMaterialsConsumptionGroupModel>> SearchProductMaterialsConsumptionGroup(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _stockDbContext.ProductMaterialsConsumptionGroup.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.Title.Contains(keyword) || x.ProductMaterialsConsumptionGroupCode.Contains(keyword));
            }

            var total = query.Count();
            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query)
                .ProjectTo<ProductMaterialsConsumptionGroupModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (lst, total);
        }
    }
}
