using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestPart
{
    public class RequestOutsourcePartDetailModel : IMapFrom<OutsourcePartRequestDetail>
    {
        public long OutsourcePartRequestDetailId { get; set; }
        public long OutsourcePartRequestId { get; set; }
        [Required(ErrorMessage = "Mã chi tiết là bắt buộc")]
        public int ProductPartId { get; set; }
        [Required(ErrorMessage = "Vị trí của chi tiết trong BOM là bắt buộc")]
        public string PathProductIdInBom { get; set; }
        [Required(ErrorMessage ="Giá trị số lượng là bắt buộc")]
        [Range(0.00001, double.MaxValue, ErrorMessage ="Số lượng phải lớn hơn 0")]
        public decimal Quantity { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourcePartRequestDetail, RequestOutsourcePartDetailModel>()
                .ForMember(m => m.ProductPartId, v => v.MapFrom(m => m.ProductId))
                .ReverseMap()
                .ForMember(m => m.ProductId, v => v.MapFrom(m => m.ProductPartId));
        }
    }

    public class OutsourcePartRequestDetailInfo: RequestOutsourcePartDetailModel
    {
        public string OutsourcePartRequestCode { get; set; }
        public long ProductionOrderDetailId { get; set; }
        public long OutsourcePartRequestDate { get; set; }
        public long OutsourcePartRequestFinishDate { get; set; }
        public string ProductionOrderCode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public string ProductPartName { get; set; }
        public string ProductPartCode { get; set; }
        public string ProductPartTitle { get; set; }
        public string OrderCode { get; set; }
        public int ProductId { get; set; }
        public decimal ProductOrderDetailQuantity { get; set; }
        public string ProductTitle { get; set; }
        public decimal QuantityProcessed { get; set; }
        public EnumOutsourceRequestStatusType OutsourcePartRequestDetailStatusId { get; set; }
    }


}
