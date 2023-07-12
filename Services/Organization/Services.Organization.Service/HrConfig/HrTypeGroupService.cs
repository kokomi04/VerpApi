using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Organization.Model.HrConfig;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Master.Config.ActionButton;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Organization.Service.HrConfig
{
    public interface IHrTypeGroupService
    {
        Task<int> HrTypeGroupCreate(HrTypeGroupModel model);
        Task<bool> HrTypeGroupDelete(int hrTypeGroupId);
        Task<IList<HrTypeGroupList>> HrTypeGroupList();
        Task<bool> HrTypeGroupUpdate(int hrTypeGroupId, HrTypeGroupModel model);
    }

    public class HrTypeGroupService : IHrTypeGroupService
    {
        private readonly ILogger _logger;
        private readonly ObjectActivityLogFacade _objActivityLogFacade;
        private readonly IMapper _mapper;
        private readonly OrganizationDBContext _organizationDBContext;

        public HrTypeGroupService(IMapper mapper, OrganizationDBContext organizationDBContext, ILogger<HrTypeGroupService> logger, IActivityLogService activityLogService)
        {
            _mapper = mapper;
            _organizationDBContext = organizationDBContext;
            _logger = logger;
            _objActivityLogFacade = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ActionButton);
        }

        public async Task<int> HrTypeGroupCreate(HrTypeGroupModel model)
        {
            var info = _mapper.Map<HrTypeGroup>(model);
            await _organizationDBContext.HrTypeGroup.AddAsync(info);
            await _organizationDBContext.SaveChangesAsync();

            await _objActivityLogFacade.LogBuilder(() => ActionButtonActivityLogMessage.Create)
                   .MessageResourceFormatDatas($"Thêm nhóm chứng từ hành chính nhân sự {info.HrTypeGroupName}")
                   .ObjectId(info.HrTypeGroupId)
                   .ObjectType(EnumObjectType.HrTypeGroup)
                   .JsonData(model)
                   .CreateLog();

            return info.HrTypeGroupId;
        }

        public async Task<bool> HrTypeGroupUpdate(int hrTypeGroupId, HrTypeGroupModel model)
        {
            var info = await _organizationDBContext.HrTypeGroup.FirstOrDefaultAsync(g => g.HrTypeGroupId == hrTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm chứng từ hành chính nhân sự không tồn tại");

            _mapper.Map(model, info);

            await _organizationDBContext.SaveChangesAsync();

            await _objActivityLogFacade.LogBuilder(() => ActionButtonActivityLogMessage.Update)
                   .MessageResourceFormatDatas($"Cập nhật nhóm chứng từ hành chính nhân sự {info.HrTypeGroupName}")
                   .ObjectId(info.HrTypeGroupId)
                   .ObjectType(EnumObjectType.HrTypeGroup)
                   .JsonData(model)
                   .CreateLog();
            return true;
        }

        public async Task<bool> HrTypeGroupDelete(int hrTypeGroupId)
        {
            var info = await _organizationDBContext.HrTypeGroup.FirstOrDefaultAsync(g => g.HrTypeGroupId == hrTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm chứng từ hành chính nhân sự không tồn tại");

            info.IsDeleted = true;

            await _organizationDBContext.SaveChangesAsync();

            await _objActivityLogFacade.LogBuilder(() => ActionButtonActivityLogMessage.Update)
                   .MessageResourceFormatDatas($"Xóa nhóm chứng từ hành chính nhân sự {info.HrTypeGroupName}")
                   .ObjectId(info.HrTypeGroupId)
                   .ObjectType(EnumObjectType.HrTypeGroup)
                   .JsonData(new { hrTypeGroupId })
                   .CreateLog();

            return true;
        }

        public async Task<IList<HrTypeGroupList>> HrTypeGroupList()
        {
            return await _organizationDBContext.HrTypeGroup.ProjectTo<HrTypeGroupList>(_mapper.ConfigurationProvider).OrderBy(g => g.SortOrder).ToListAsync();
        }
    }
}