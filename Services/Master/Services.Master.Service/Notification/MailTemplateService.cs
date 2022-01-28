using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Notification;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using System.ComponentModel;

namespace VErp.Services.Master.Service.Notification
{
    public interface IMailTemplateService
    {
        Task<int> AddMailTemplate(MailTemplateModel model);
        Task<bool> DelteMailTemplate(int mailTemplateId);
        Task<IList<MailTemplateModel>> GetListMailTemplate();
        Task<MailTemplateModel> GetMailTemplate(int mailTemplateId);
        Task<MailTemplateModel> GetMailTemplateByCode(string code);
        Task<bool> UpdateMailTemplate(int mailTemplateId, MailTemplateModel model);
        Task<IList<TemplateMailField>> GetTemplateMailFields();

    }

    public class MailTemplateService : IMailTemplateService
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly ObjectActivityLogFacade _activityLog;
        private readonly IMapper _mapper;

        public MailTemplateService(MasterDBContext masterDBContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _masterDBContext = masterDBContext;
            _mapper = mapper;
            _activityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.MailTemplate);
        }

        public async Task<IList<MailTemplateModel>> GetListMailTemplate()
        {
            return await _masterDBContext.MailTemplate.AsNoTracking().ProjectTo<MailTemplateModel>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<MailTemplateModel> GetMailTemplate(int mailTemplateId)
        {
            var entity = await _masterDBContext.MailTemplate.FirstOrDefaultAsync(x => x.MailTemplateId == mailTemplateId);
            if (entity == null)
                throw new BadRequestException(MailTemplateErrorCode.NotFoundMailTemplate);

            return _mapper.Map<MailTemplateModel>(entity);
        }

        public async Task<MailTemplateModel> GetMailTemplateByCode(string code)
        {
            var entity = await _masterDBContext.MailTemplate.FirstOrDefaultAsync(x => x.TemplateCode == code);
            if (entity == null)
                throw new BadRequestException(MailTemplateErrorCode.NotFoundMailTemplate);

            return _mapper.Map<MailTemplateModel>(entity);
        }

        public async Task<int> AddMailTemplate(MailTemplateModel model)
        {
            var existsTemplateCode = await HasMailTemplateInSystem(model.TemplateCode);
            if (existsTemplateCode)
                throw new BadRequestException(MailTemplateErrorCode.ExistsTemplateCode);

            var entity = _mapper.Map<MailTemplate>(model);
            _masterDBContext.MailTemplate.Add(entity);
            await _masterDBContext.SaveChangesAsync();

            return entity.MailTemplateId;
        }
        public async Task<bool> UpdateMailTemplate(int mailTemplateId, MailTemplateModel model)
        {
            var entity = await _masterDBContext.MailTemplate.FirstOrDefaultAsync(x => x.MailTemplateId == mailTemplateId);
            if (entity == null)
                throw new BadRequestException(MailTemplateErrorCode.NotFoundMailTemplate);

            if (entity.TemplateCode != model.TemplateCode)
            {
                var existsTemplateCode = await HasMailTemplateInSystem(model.TemplateCode);
                if (existsTemplateCode)
                    throw new BadRequestException(MailTemplateErrorCode.ExistsTemplateCode);
            }

            _mapper.Map(model, entity);
            await _masterDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DelteMailTemplate(int mailTemplateId)
        {
            var entity = await _masterDBContext.MailTemplate.FirstOrDefaultAsync(x => x.MailTemplateId == mailTemplateId);
            if (entity == null)
                throw new BadRequestException(MailTemplateErrorCode.NotFoundMailTemplate);

            entity.IsDeleted = true;

            await _masterDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<IList<TemplateMailField>> GetTemplateMailFields()
        {
            var fields = new List<TemplateMailField>();

            foreach (var prop in typeof(ObjectDataTemplateMail).GetProperties())
            {
                var attributes = (DescriptionAttribute[])prop.GetCustomAttributes(typeof(DescriptionAttribute), false);

                var title = (attributes.Length > 0) ? attributes[0].Description : prop.Name;
                var fieldName = prop.Name;


                fields.Add(new TemplateMailField() { Title = title, FieldName = fieldName });
            }
            return await Task.FromResult(fields);
        }

        private async Task<bool> HasMailTemplateInSystem(string templateCode)
        {
            return await _masterDBContext.MailTemplate.AnyAsync(x => x.TemplateCode == templateCode);
        }
    }
}