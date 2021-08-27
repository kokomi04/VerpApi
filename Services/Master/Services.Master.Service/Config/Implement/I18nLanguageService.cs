using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Config;

namespace VErp.Services.Master.Service.Config.Implement
{
    public interface II18nLanguageService
    {
        Task<long> AddMissingKeyI18n(string key);
        Task<bool> DeleteI18n(IList<long> Ids);
        Task<NonCamelCaseDictionary> GetI18nByLanguage(string language);
        Task<PageData<I18nLanguageModel>> SearchI18n(string keyword, int size, int page);
        Task<bool> UpdateI18n(IList<I18nLanguageModel> models);
    }

    public class I18nLanguageService : II18nLanguageService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly IMapper _mapper;
        private readonly IActivityLogService _activityLogService;

        public I18nLanguageService(MasterDBContext masterDbContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _masterDbContext = masterDbContext;
            _mapper = mapper;
            _activityLogService = activityLogService;
        }

        public async Task<PageData<I18nLanguageModel>> SearchI18n(string keyword, int size, int page)
        {
            var query = _masterDbContext.I18nLanguage.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.Key.Contains(keyword));
            }

            var total = await query.CountAsync();
            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query)
            .ProjectTo<I18nLanguageModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
            return (lst, total);
        }

        public async Task<NonCamelCaseDictionary> GetI18nByLanguage(string language)
        {
            return (await _masterDbContext.I18nLanguage.ToListAsync()).ToNonCamelCaseDictionary(k => k.Key, v =>
            {
                Type type = v.GetType();
                var property = type.GetProperties().FirstOrDefault(x => x.Name.ToLower() == language.ToLower());
                if (property != null)
                    return property.GetValue(v, null);
                return null;
            });
        }

        public async Task<long> AddMissingKeyI18n(string key)
        {
            var entity = new I18nLanguage
            {
                Key = key,
            };

            _masterDbContext.I18nLanguage.Add(entity);

            await _masterDbContext.SaveChangesAsync();

            return entity.I18nLanguageId;
        }

        public async Task<bool> UpdateI18n(IList<I18nLanguageModel> models)
        {
            var existsI18n = await _masterDbContext.I18nLanguage.Where(i => models.Select(x => x.I18nLanguageId).Contains(i.I18nLanguageId)).ToListAsync();
            var addI18n = models.Where(x => x.I18nLanguageId <= 0).ToList();

            foreach (var e in existsI18n)
            {
                var model = models.FirstOrDefault(x => x.I18nLanguageId == e.I18nLanguageId);

                _mapper.Map(model, e);
            }

            await _masterDbContext.I18nLanguage.AddRangeAsync(_mapper.Map<IList<I18nLanguage>>(addI18n));

            await _masterDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteI18n(IList<long> Ids)
        {
            var existsI18n = await _masterDbContext.I18nLanguage.Where(i => Ids.Contains(i.I18nLanguageId)).ToListAsync();

            foreach (var e in existsI18n)
            {
                e.IsDeleted = true;
            }

            await _masterDbContext.SaveChangesAsync();

            return true;
        }
    }
}