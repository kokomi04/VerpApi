using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.Caching;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Config;

namespace VErp.Services.Master.Service.Config.Implement
{
    public interface II18nLanguageService
    {
        Task<long> AddI18n(I18nLanguageModel model);
        Task<long> AddMissingKeyI18n(string key);
        Task<bool> DeleteI18n(long id);
        Task<NonCamelCaseDictionary> GetI18nByLanguage(string language);
        Task<PageData<I18nLanguageModel>> SearchI18n(string keyword, int size, int page);
        Task<bool> UpdateI18n(long i18nLanguageId, I18nLanguageModel model);
    }

    public class I18nLanguageService : II18nLanguageService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly IMapper _mapper;
        private readonly IActivityLogService _activityLogService;
        private readonly ICachingService _cachingService;

        public I18nLanguageService(MasterDBContext masterDbContext, IMapper mapper, IActivityLogService activityLogService, ICachingService cachingService)
        {
            _masterDbContext = masterDbContext;
            _mapper = mapper;
            _activityLogService = activityLogService;
            _cachingService = cachingService;
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
            return await _cachingService.TryGetSet("i18n", "languge_" + language, TimeSpan.FromDays(7), async () =>
            {
                return (await _masterDbContext.I18nLanguage.ToListAsync())
                    .GroupBy(x => x.Key)
                    .Select(x => x.First())
                    .ToNonCamelCaseDictionary(k => k.Key, v =>
                    {
                        Type type = v.GetType();
                        var property = type.GetProperties().FirstOrDefault(x => x.Name.ToLower() == language.ToLower());
                        if (property != null)
                        {
                            var value = property.GetValue(v, null);

                            if (null == value || string.IsNullOrEmpty(value.ToString())) value = v.Key;

                            return value;
                        }

                        return v.Key;
                    });
            });

        }

        public async Task<long> AddMissingKeyI18n(string key)
        {
            var hasKey = await _masterDbContext.I18nLanguage.AnyAsync(x => x.Key == key);
            if (hasKey)
                return 0;

            var model = new I18nLanguageModel
            {
                Key = key,
                Vi = key,
                //En = $"{key} (En)"
            };

            return await AddI18n(model);
        }

        public async Task<long> AddI18n(I18nLanguageModel model)
        {

            var hasKey = await _masterDbContext.I18nLanguage.AnyAsync(x => x.Key == model.Key);
            if (hasKey)
                throw new BadRequestException(I18nLanguageErrorCode.AlreadyExistsKeyCode);

            var entity = _mapper.Map<I18nLanguage>(model);
            _masterDbContext.I18nLanguage.Add(entity);

            await _masterDbContext.SaveChangesAsync();

            return entity.I18nLanguageId;
        }

        public async Task<bool> UpdateI18n(long i18nLanguageId, I18nLanguageModel model)
        {
            var existsI18n = await _masterDbContext.I18nLanguage.Where(i => i.I18nLanguageId == i18nLanguageId).FirstOrDefaultAsync();
            if (existsI18n == null)
                throw new BadRequestException(I18nLanguageErrorCode.ItemNotFound);

            if (existsI18n.Key != model.Key)
            {
                var hasKey = await _masterDbContext.I18nLanguage.AnyAsync(x => x.Key == model.Key);
                if (hasKey)
                    throw new BadRequestException(I18nLanguageErrorCode.AlreadyExistsKeyCode);
            }

            existsI18n.Vi = model.Vi;
            existsI18n.En = model.En;
            existsI18n.Key = model.Key;

            await _masterDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteI18n(long id)
        {
            var existsI18n = await _masterDbContext.I18nLanguage.Where(i => i.I18nLanguageId == id).FirstOrDefaultAsync();
            if (existsI18n == null)
                throw new BadRequestException(I18nLanguageErrorCode.ItemNotFound);

            existsI18n.IsDeleted = true;

            await _masterDbContext.SaveChangesAsync();

            return true;
        }
    }
}