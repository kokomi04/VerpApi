using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.PrintConfig;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace VErp.Services.Master.Service.PrintConfig.Implement
{
    public abstract class PrintConfigHeaderServiceAbstract<TEntity, TModel, TViewModel>
        where TEntity : class, IPrintConfigHeaderEntity
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly string _configIdFieldName;

        public PrintConfigHeaderServiceAbstract(MasterDBContext masterDBContext,
                    ILogger<PrintConfigHeaderServiceAbstract<TEntity, TModel, TViewModel>> logger,
                    IMapper mapper,
                    string configIdFieldName)
        {
            _masterDBContext = masterDBContext;
            _logger = logger;
            _mapper = mapper;
            _configIdFieldName = configIdFieldName;
        }


        protected abstract Task LogAddPrintConfigHeader(TModel model, TEntity entity);
        protected abstract Task LogUpdatePrintConfigHeader(TModel model, TEntity entity);
        protected abstract Task LogDeletePrintConfigHeader(TEntity entity);

        public async Task<PageData<TViewModel>> Search(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _masterDBContext.Set<TEntity>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.Title.Contains(keyword));
            
            var total = await query.CountAsync();
            var lst = await(size > 0 ? (query.Skip((page - 1) * size)).Take(size) : query)
                .OrderBy(x=>x.SortOrder)
                .ProjectTo<TViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (lst, total);
        }
        public async Task<TModel> GetHeaderById(int headerId)
        {
            var header = await _masterDBContext.Set<TEntity>().FindAsync(headerId);
            if (header == null) 
                throw new BadRequestException("Không tìm thấy cấu hình header phiếu in");

            return _mapper.Map<TModel>(header);

        }
        public async Task<int> CreateHeader(TModel model)
        {
            await using var trans = await _masterDBContext.Database.BeginTransactionAsync();

            try
            {
                var entity = _mapper.Map<TEntity>(model);

                await _masterDBContext.Set<TEntity>().AddAsync(entity);
                await _masterDBContext.SaveChangesAsync();
                await trans.CommitAsync();

                var configId = ConfigId().Compile().Invoke(entity);

                await LogAddPrintConfigHeader(model, entity);

                return configId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreatePrintConfigHeader");
                throw;
            }
        }
        public async Task<bool> UpdateHeader(int headerId, TModel model)
        {
            await using var trans = await _masterDBContext.Database.BeginTransactionAsync();

            try
            {
                var header = await _masterDBContext.Set<TEntity>().FindAsync(headerId);

                if (header == null) 
                    throw new BadRequestException("Không tìm thấy cấu hình header phiếu in");

                _mapper.Map(model, header);

                await _masterDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await LogUpdatePrintConfigHeader(model, header);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdatePrintConfigHeader");
                throw;
            }
        }

        public async Task<bool> DeleteHeader(int headerId)
        {
            await using var trans = await _masterDBContext.Database.BeginTransactionAsync();

            try
            {
                var header = await _masterDBContext.Set<TEntity>().FindAsync(headerId);

                if (header == null)
                    throw new BadRequestException("Không tìm thấy cấu hình header phiếu in");

                header.IsDeleted = true;

                await _masterDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await LogDeletePrintConfigHeader(header);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeletePrintConfigHeader");
                throw;
            }
        }

        private Expression<Func<T, int>> SelectField<T>(string fieldName)
        {
            var entity = Expression.Parameter(typeof(T));
            var key = Expression.PropertyOrField(entity, fieldName);
            return Expression.Lambda<Func<T, int>>(key, entity);
        }

        private Expression<Func<TEntity, int>> ConfigId()
        {
            return SelectField<TEntity>(_configIdFieldName);
        }
    }
}
