using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Organization.Model.HrConfig;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
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
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly OrganizationDBContext _organizationDBContext;

        public HrTypeGroupService(IMapper mapper, OrganizationDBContext organizationDBContext, ILogger<HrTypeGroupService> logger, IActivityLogService activityLogService)
        {
            _mapper = mapper;
            _organizationDBContext = organizationDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<int> HrTypeGroupCreate(HrTypeGroupModel model)
        {
            var info = _mapper.Map<HrTypeGroup>(model);
            await _organizationDBContext.HrTypeGroup.AddAsync(info);
            await _organizationDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.HrTypeGroup, info.HrTypeGroupId, $"Thêm nhóm chứng từ hành chính nhân sự {info.HrTypeGroupName}", model.JsonSerialize());

            return info.HrTypeGroupId;
        }

        public async Task<bool> HrTypeGroupUpdate(int hrTypeGroupId, HrTypeGroupModel model)
        {
            var info = await _organizationDBContext.HrTypeGroup.FirstOrDefaultAsync(g => g.HrTypeGroupId == hrTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm chứng từ hành chính nhân sự không tồn tại");

            _mapper.Map(model, info);

            await _organizationDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.HrTypeGroup, info.HrTypeGroupId, $"Cập nhật nhóm chứng từ hành chính nhân sự {info.HrTypeGroupName}", model.JsonSerialize());

            return true;
        }

        public async Task<bool> HrTypeGroupDelete(int hrTypeGroupId)
        {
            var info = await _organizationDBContext.HrTypeGroup.FirstOrDefaultAsync(g => g.HrTypeGroupId == hrTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm chứng từ hành chính nhân sự không tồn tại");

            info.IsDeleted = true;

            await _organizationDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.HrTypeGroup, info.HrTypeGroupId, $"Xóa nhóm chứng từ hành chính nhân sự {info.HrTypeGroupName}", new { hrTypeGroupId }.JsonSerialize());

            return true;
        }

        public async Task<IList<HrTypeGroupList>> HrTypeGroupList()
        {
            return await _organizationDBContext.HrTypeGroup.ProjectTo<HrTypeGroupList>(_mapper.ConfigurationProvider).OrderBy(g => g.SortOrder).ToListAsync();
        }
    }
}