using AutoMapper;
using VErp.Commons.GlobalObject;
using NotificationEntity = ActivityLogDB.Notification;

namespace VErp.Services.Master.Model.Notification
{
    public class NotificationModel : IMapFrom<NotificationEntity>
    {
        public long NotificationId { get; set; }
        public int UserId { get; set; }
        public long UserActivityLogId { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public long? ReadDateTimeUtc { get; set; }
        public bool IsRead { get; set; }
        public long SubscriptionId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<NotificationModel, NotificationEntity>()
            .ForMember(x => x.ReadDateTimeUtc, v => v.MapFrom(m => m.ReadDateTimeUtc.UnixToDateTime()))
            .ForMember(x => x.CreatedDatetimeUtc, v => v.MapFrom(m => m.CreatedDatetimeUtc.UnixToDateTime()))
            .ReverseMap()
            .ForMember(x => x.CreatedDatetimeUtc, v => v.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
            .ForMember(x => x.ReadDateTimeUtc, v => v.MapFrom(m => m.ReadDateTimeUtc.GetUnix()));
        }
    }


}