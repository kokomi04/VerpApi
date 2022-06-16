using ActivityLogDB;
using AutoMapper;
using VErp.Commons.Enums;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;

namespace VErp.Infrastructure.ServiceCore.Model
{
    public class UserLoginLogModel : IMapFrom<UserLoginLog>
    {
        public long UserLoginLogId { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public EnumUserLoginStatus Status { get; set; }
        public EnumMessageType MessageTypeId { get; set; }
        public string MessageResourceName { get; set; }
        public string MessageResourceFormatData { get; set; }
        public string Message { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public string StrSubId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UserLoginLog, UserLoginLogModel>()
                .ForMember(d => d.Status, s => s.MapFrom(m => (EnumUserLoginStatus)m.Status))
                .ForMember(d => d.MessageTypeId, s => s.MapFrom(m => (EnumCustomerType)m.MessageTypeId))
                .ForMember(d => d.CreatedDatetimeUtc, s => s.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
                .ReverseMap()
                .ForMember(d => d.Status, s => s.MapFrom(m => (int)m.Status))
                .ForMember(d => d.MessageTypeId, s => s.MapFrom(m => (int)m.MessageTypeId))
                .ForMember(d => d.CreatedDatetimeUtc, s => s.MapFrom(m => m.CreatedDatetimeUtc.UnixToDateTime()));
        }
    }
}
