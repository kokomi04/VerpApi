//using System;
//using System.Collections.Generic;

//namespace VErp.Infrastructure.EF.ManufacturingDB
//{
//    public partial class ProductionSchedule
//    {
//        public long ProductionScheduleId { get; set; }
//        public DateTime StartDate { get; set; }
//        public DateTime EndDate { get; set; }
//        public int ProductionScheduleStatus { get; set; }
//        public long ProductionOrderDetailId { get; set; }
//        public decimal ProductionScheduleQuantity { get; set; }
//        public int CreatedByUserId { get; set; }
//        public DateTime CreatedDatetimeUtc { get; set; }
//        public int UpdatedByUserId { get; set; }
//        public DateTime UpdatedDatetimeUtc { get; set; }
//        public bool IsDeleted { get; set; }
//        public DateTime? DeletedDatetimeUtc { get; set; }
//        public int SubsidiaryId { get; set; }
//        public long ScheduleTurnId { get; set; }
//        public string ScheduleCode { get; set; }

//        public virtual ProductionOrderDetail ProductionOrderDetail { get; set; }
//    }
//}
