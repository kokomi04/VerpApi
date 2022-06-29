
using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public interface IRefForeginKey
    {
        void SetRefForeginKey(int[] refForeginKey);
        int GetPrimaryKey();
    }

    public interface IInnerBySelf<T> where T : class
    {
        bool HasInner();
        IList<T> GetInner();
    }

    public class ArrangeShiftModel : IMapFrom<ArrangeShift>, IRefForeginKey
    {
        public int ArrangeShiftId { get; set; }
        public EnumArrangeShiftMode ArrangeShiftMode { get; set; }
        public int WorkScheduleId { get; set; }
        public int OrdinalNumber { get; set; }

        public IList<ArrangeShiftItemModel> Items { get; set; }

        public int GetPrimaryKey()
        {
            return ArrangeShiftId;
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ArrangeShift, ArrangeShiftModel>()
            .ForMember(x => x.Items, v => v.MapFrom(m => m.ArrangeShiftItem))
            .ReverseMap()
            .ForMember(x => x.ArrangeShiftItem, v => v.Ignore());
        }

        public void SetRefForeginKey(int[] refForeginKey)
        {
            ArrangeShiftId = 0;
            if (refForeginKey.Length > 0)
            {
                this.WorkScheduleId = refForeginKey[0];
            }
        }
    }

    public class ArrangeShiftItemModel : IMapFrom<ArrangeShiftItem>, IRefForeginKey, IInnerBySelf<ArrangeShiftItemModel>
    {
        public ArrangeShiftItemModel()
        {
            InnerItems = new List<ArrangeShiftItemModel>();
        }
        public int ArrangeShiftItemId { get; set; }
        public int ArrangeShiftId { get; set; }
        public int? ShiftConfigurationId { get; set; }
        public int? OrdinalNumber { get; set; }
        public int? ParentArrangeShiftItemId { get; set; }

        public IList<ArrangeShiftItemModel> InnerItems { get; set; }

        public IList<ArrangeShiftItemModel> GetInner()
        {
            return InnerItems;
        }

        public int GetPrimaryKey()
        {
            return ArrangeShiftItemId;
        }

        public bool HasInner()
        {
            return InnerItems.Count > 0;
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ArrangeShiftItem, ArrangeShiftItemModel>()
            .ForMember(x => x.InnerItems, v => v.MapFrom(m => m.InverseParentArrangeShiftItem))
            .ReverseMap()
            .ForMember(x => x.InverseParentArrangeShiftItem, v => v.Ignore());
        }

        public void SetRefForeginKey(int[] refForeginKey)
        {
            ArrangeShiftItemId = 0;

            if (refForeginKey.Length > 0)
            {
                this.ArrangeShiftId = refForeginKey[0];
            }

            if (refForeginKey.Length > 1)
            {
                this.ParentArrangeShiftItemId = refForeginKey[1];
            }
        }
    }
}