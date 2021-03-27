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
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductMaterialsConsumptionGroupService : IProductMaterialsConsumptionGroupService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;


        public ProductMaterialsConsumptionGroupService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductBomService> logger
            , IActivityLogService activityLogService
            , IMapper mapper)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<int> AddProductMaterialsConsumptionGroup(ProductMaterialsConsumptionGroupModel model)
        {
            var group = _stockDbContext.ProductMaterialsConsumptionGroup.AsNoTracking().FirstOrDefault(x => x.ProductMaterialsConsumptionGroupCode == model.ProductMaterialsConsumptionGroupCode);
            if (group != null)
                throw new BadRequestException(GeneralCode.GeneralError, "Đã tồn tại mã nhóm vật tư tiêu hao");

            var entity = _mapper.Map<ProductMaterialsConsumptionGroup>(model);
            _stockDbContext.ProductMaterialsConsumptionGroup.Add(entity);
            await _stockDbContext.SaveChangesAsync();

            return entity.ProductMaterialsConsumptionGroupId;
        }

        public async Task<bool> UpdateProductMaterialsConsumptionGroup(int groupId, ProductMaterialsConsumptionGroupModel model)
        {
            var group = _stockDbContext.ProductMaterialsConsumptionGroup.FirstOrDefault(x => x.ProductMaterialsConsumptionGroupId == groupId);
            if (group != null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy vật tư tiêu hao");

            if(group.ProductMaterialsConsumptionGroupCode != model.ProductMaterialsConsumptionGroupCode)
            {
                var check = _stockDbContext.ProductMaterialsConsumptionGroup.AsNoTracking()
                    .FirstOrDefault(x => x.ProductMaterialsConsumptionGroupCode == model.ProductMaterialsConsumptionGroupCode
                        && groupId != x.ProductMaterialsConsumptionGroupId);
                if (check != null)
                    throw new BadRequestException(GeneralCode.GeneralError, "Đã tồn tại mã nhóm vật tư tiêu hao");
            }

            _mapper.Map(model, group);
            await _stockDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductMaterialsConsumptionGroup(int groupId)
        {
            var group = _stockDbContext.ProductMaterialsConsumptionGroup.FirstOrDefault(x => x.ProductMaterialsConsumptionGroupId == groupId);
            if (group != null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy vật tư tiêu hao");
            group.IsDeleted = true;
            await _stockDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<ProductMaterialsConsumptionGroupModel> GetProductMaterialsConsumptionGroup(int groupId)
        {
            var group = await _stockDbContext.ProductMaterialsConsumptionGroup.AsNoTracking().FirstOrDefaultAsync(x => x.ProductMaterialsConsumptionGroupId == groupId);
            if (group != null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy vật tư tiêu hao");

            return _mapper.Map<ProductMaterialsConsumptionGroupModel>(group);
        }

        public async Task<PageData<ProductMaterialsConsumptionGroupModel>> SearchProductMaterialsConsumptionGroup(string keyword, int page, int size)
        {
            var query = _stockDbContext.ProductMaterialsConsumptionGroup.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.Title.Contains(keyword) || x.ProductMaterialsConsumptionGroupCode.Contains(keyword));
            }

            var total = query.Count();
            var lst = (size > 0 ? query.Skip((page - 1) * size).Take(size) : query)
                .ProjectTo<ProductMaterialsConsumptionGroupModel>(_mapper.ConfigurationProvider)
                .ToList();

            return (lst, total);
        }
    }
}
