using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductSemi;
using ProductSemiEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductSemi;

namespace VErp.Services.Manafacturing.Service.ProductSemi.Implement
{
    public class ProductSemiService: IProductSemiService
    {
        private readonly ManufacturingDBContext _manuDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ProductSemiService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductSemiService> logger
            , IMapper mapper)
        {
            _manuDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<long> CreateProductSemi(ProductSemiModel model)
        {
            var data = _mapper.Map<ProductSemiEntity>(model);
            await _manuDBContext.ProductSemi.AddAsync(_mapper.Map<ProductSemiEntity>(model));
            await _manuDBContext.SaveChangesAsync();
            return data.ProductSemiId;
        }

        public async Task<bool> DeleteProductSemi(long productSemiId)
        {
            var data = await _manuDBContext.ProductSemi.FirstOrDefaultAsync(p => p.ProductSemiId == productSemiId);
            if (data == null)
                throw new BadRequestException(ProductSemiErrorCode.NotFoundProductSemi);
            data.IsDeleted = true;
            await _manuDBContext.SaveChangesAsync();
            return true;
        }

        public async Task<IList<ProductSemiModel>> GetListProductSemis(int containerId, int containerType)
        {
            var ls = await _manuDBContext.ProductSemi.Where(x => x.ContainerId == containerId && x.ContainerTypeId == containerType)
                .ProjectTo<ProductSemiModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return ls;
        }

        public async Task<bool> UpdateProductSemi(long productSemiId, ProductSemiModel model)
        {
            var source = await _manuDBContext.ProductSemi.FirstOrDefaultAsync(p => p.ProductSemiId == productSemiId);
            if (source == null)
                throw new BadRequestException(ProductSemiErrorCode.NotFoundProductSemi);

            _mapper.Map(model, source);
            await _manuDBContext.SaveChangesAsync();
            return true;
        }
    }
}
