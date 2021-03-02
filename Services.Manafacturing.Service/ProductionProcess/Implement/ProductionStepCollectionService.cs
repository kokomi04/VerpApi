using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionStep;

namespace VErp.Services.Manafacturing.Service.ProductionProcess.Implement
{
    public class ProductionStepCollectionService: IProductionStepCollectionService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ProductionStepCollectionService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionStepCollectionService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<long> AddProductionStepCollection(ProductionStepCollectionModel model)
        {
            var collections = (await _manufacturingDBContext.ProductionStepCollection.AsNoTracking()
                .ProjectTo<ProductionStepCollectionModel>(_mapper.ConfigurationProvider)
                .ToListAsync())
                .Select(x => new
                {
                    x.ProductionStepCollectionId,
                    collection = string.Join("-", x.Collections.OrderBy(c => c.Order).Select(c => c.StepId))
                });
            var collection = string.Join("-", model.Collections.OrderBy(c => c.Order).Select(c => c.StepId));

            var entity = new ProductionStepCollection();
            if (collections.Count() > 0 && collections.Any(x => x.collection.Equals(collection)))
            {
                var item = collections.FirstOrDefault(x => x.collection.Equals(collection));
                entity = await _manufacturingDBContext.ProductionStepCollection.FirstOrDefaultAsync(x => x.ProductionStepCollectionId == item.ProductionStepCollectionId);
                entity.Frequence++;
            }
            else
            {
                entity = _mapper.Map<ProductionStepCollection>(model);
                entity.Frequence = 1;
                _manufacturingDBContext.ProductionStepCollection.Add(entity);
            }
            
            await _manufacturingDBContext.SaveChangesAsync();

            return entity.ProductionStepCollectionId;
        }

        public async Task<bool> DeleteProductionStepCollection(long productionStepCollectionId)
        {
            var entity = await _manufacturingDBContext.ProductionStepCollection.FirstOrDefaultAsync(x => x.ProductionStepCollectionId == productionStepCollectionId);
            if (entity == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);
            entity.IsDeleted = true;
            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }

        public async Task<ProductionStepCollectionModel> GetProductionStepCollection(long productionStepCollectionId)
        {
            var entity = await _manufacturingDBContext.ProductionStepCollection.FirstOrDefaultAsync(x => x.ProductionStepCollectionId == productionStepCollectionId);
            if (entity == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);
            entity.Frequence++;
            await _manufacturingDBContext.SaveChangesAsync();

            return _mapper.Map<ProductionStepCollectionModel>(entity);
        }

        public async Task<PageData<ProductionStepCollectionSearch>> SearchProductionStepCollection(string keyword, int page, int size)
        {
            var collections = await _manufacturingDBContext.ProductionStepCollection.AsNoTracking()
                .OrderByDescending(x => x.Frequence).ThenBy(x => x.ProductionStepCollectionId)
                .ProjectTo<ProductionStepCollectionSearch>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var stepInfos = await _manufacturingDBContext.Step.AsNoTracking()
                .Where(x => collections.SelectMany(x => x.Collections).Select(x => x.StepId).Distinct().Contains(x.StepId))
                .ToListAsync();

            collections.ForEach(x =>
            {
                foreach (var c in x.Collections)
                {
                    var stepInfo = stepInfos.FirstOrDefault(x => x.StepId == c.StepId);
                    if (stepInfo == null)
                        c.StepName = string.Empty;
                    else c.StepName = stepInfo.StepName;
                }
                x.Description = string.Join(", ", x.Collections.OrderBy(c => c.Order).Select(c => c.StepName));
            });

            var query = collections.AsQueryable();

            var total = query.Count();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.Title.Contains(keyword) || x.Description.Contains(keyword));

            var lst = (size > 0 ? query.Skip((page - 1) * size).Take(size) : query).ToList();

            return (lst, total);
        }

        public async Task<bool> UpdateProductionStepCollection(long productionStepCollectionId, ProductionStepCollectionModel model)
        {
            var entity = await _manufacturingDBContext.ProductionStepCollection.FirstOrDefaultAsync(x => x.ProductionStepCollectionId == productionStepCollectionId);
            if (entity == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);
            _mapper.Map(model, entity);
            await _manufacturingDBContext.SaveChangesAsync();

            return true;

        }
    }
}
