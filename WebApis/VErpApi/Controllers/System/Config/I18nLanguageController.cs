using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Service.Config;
using VErp.Services.Master.Service.Config.Implement;

namespace VErpApi.Controllers.System
{

    [Route("api/I18nLanguage")]
    public class I18nLanguageController : VErpBaseController
    {
        private readonly II18nLanguageService _i18NLanguageService;

        public I18nLanguageController(II18nLanguageService i18NLanguageService)
        {
            _i18NLanguageService = i18NLanguageService;
        }

        [HttpPost, Route("missingKey")]
        public async Task<long> AddMissingKeyI18n([FromQuery] string key)
        {
            return await _i18NLanguageService.AddMissingKeyI18n(key);
        }

        [HttpDelete, Route("")]
        public async Task<bool> DeleteI18n([FromBody] IList<long> i18nLanguageId)
        {
            return await _i18NLanguageService.DeleteI18n(i18nLanguageId);
        }
        
        [HttpGet, Route("")]
        public async Task<NonCamelCaseDictionary> GetI18nByLanguage([FromQuery] string language)
        {
            return await _i18NLanguageService.GetI18nByLanguage(language);
        }

        [HttpPost, Route("search")]
        public async Task<PageData<I18nLanguageModel>> SearchI18n([FromQuery]string keyword, [FromQuery]int size, [FromQuery]int page)
        {
            return await _i18NLanguageService.SearchI18n(keyword, size, page);
        }

        [HttpPut, Route("")]
        public async Task<bool> UpdateI18n(IList<I18nLanguageModel> models)
        {
            return await _i18NLanguageService.UpdateI18n(models);
        }

    }
}