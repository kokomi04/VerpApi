using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Organization;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Leave;
using VErp.Services.Organization.Service.Leave;

namespace VErpApi.Controllers.System.Organization.Leave
{
    [Route("api/organization/Leave/letter")]
    public class LeaveLetterController : VErpBaseController
    {
        private readonly ILeaveLetterService _leaveLetterService;
        private readonly ICurrentContextService _currentContextService;

        public LeaveLetterController(ILeaveLetterService leaveLetterService, ICurrentContextService currentContextService)
        {
            _leaveLetterService = leaveLetterService;
            _currentContextService = currentContextService;
        }

        [HttpGet("")]
        public Task<PageData<LeaveModel>> Get([FromQuery] int? userId, [FromQuery] int? roleUserId,
            [FromQuery] string keyword, [FromQuery] int? leaveConfigId, [FromQuery] int? absenceTypeSymbolId, [FromQuery] EnumLeaveStatus? leaveStatusId,
            [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] int page, [FromQuery] int size, [FromQuery] string sortBy, [FromQuery] bool asc)
        {
            return _leaveLetterService.Get(userId, roleUserId, keyword, leaveConfigId, absenceTypeSymbolId, leaveStatusId, fromDate, toDate, page, size, sortBy, asc);
        }

        [HttpGet("me")]
        public Task<PageData<LeaveModel>> Me(
            [FromQuery] string keyword, [FromQuery] int? leaveConfigId, [FromQuery] int? absenceTypeSymbolId, [FromQuery] EnumLeaveStatus? leaveStatusId,
            [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] int page, [FromQuery] int size, [FromQuery] string sortBy, [FromQuery] bool asc)
        {
            return _leaveLetterService.Get(_currentContextService.UserId, null, keyword, leaveConfigId, absenceTypeSymbolId, leaveStatusId, fromDate, toDate, page, size, sortBy, asc);
        }


        [HttpGet("role")]
        public Task<PageData<LeaveModel>> Role(
            [FromQuery] int? userId,
            [FromQuery] string keyword, [FromQuery] int? leaveConfigId, [FromQuery] int? absenceTypeSymbolId, [FromQuery] EnumLeaveStatus? leaveStatusId,
            [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] int page, [FromQuery] int size, [FromQuery] string sortBy, [FromQuery] bool asc)
        {
            return _leaveLetterService.Get(userId, _currentContextService.UserId, keyword, leaveConfigId, absenceTypeSymbolId, leaveStatusId, fromDate, toDate, page, size, sortBy, asc);
        }

        [HttpGet("{leaveId}")]
        public Task<LeaveModel> Info([FromRoute] long leaveId)
        {
            return _leaveLetterService.Info(leaveId);
        }

        [HttpGet("InfoByOwnerOrRole/{leaveId}")]
        public Task<LeaveModel> InfoByOwnerOrRole([FromRoute] long leaveId)
        {
            return _leaveLetterService.InfoByOwnerOrRole(leaveId);
        }

        [HttpGet("me/Total")]
        public Task<LeaveByYearModel> TotalbyUser()
        {
            return _leaveLetterService.TotalByUser(_currentContextService.UserId);
        }

        [HttpGet("TotalbyUser")]
        public Task<LeaveByYearModel> TotalbyUser([FromQuery] int userId)
        {
            return _leaveLetterService.TotalByUser(userId);
        }

        [HttpPost("")]
        public Task<long> Create([FromBody] LeaveModel model)
        {
            return _leaveLetterService.Create(model);
        }

        [HttpPut("{leaveId}")]
        public Task<bool> Update([FromRoute] long leaveId, [FromBody] LeaveModel model)
        {
            return _leaveLetterService.Update(leaveId, model);
        }

        [HttpDelete("{leaveId}")]
        public Task<bool> Delete([FromRoute] long leaveId)
        {
            return _leaveLetterService.Delete(leaveId);
        }

        [HttpPut("{leaveId}/CheckAccept")]
        [VErpAction(EnumActionType.Check)]
        public Task<bool> CheckAccept([FromRoute] long leaveId)
        {
            return _leaveLetterService.CheckAccept(leaveId);
        }

        [HttpPut("{leaveId}/CheckReject")]
        [VErpAction(EnumActionType.Check)]
        public Task<bool> CheckReject([FromRoute] long leaveId)
        {
            return _leaveLetterService.CheckReject(leaveId);
        }

        [HttpPut("{leaveId}/CensorAccept")]
        [VErpAction(EnumActionType.Censor)]
        public Task<bool> CensorAccept([FromRoute] long leaveId)
        {
            return _leaveLetterService.CensorAccept(leaveId);
        }


        [HttpPut("{leaveId}/CensorReject")]
        [VErpAction(EnumActionType.Censor)]
        public Task<bool> CensorReject([FromRoute] long leaveId)
        {
            return _leaveLetterService.CensorReject(leaveId);
        }

    }
}