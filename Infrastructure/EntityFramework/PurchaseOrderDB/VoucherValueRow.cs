using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class VoucherValueRow
    {
        public long FId { get; set; }
        public int VoucherTypeId { get; set; }
        public long VoucherBillFId { get; set; }
        public int BillVersion { get; set; }
        public bool IsBillEntry { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public string SystemLog { get; set; }
        public int SubsidiaryId { get; set; }
        public string SoCt { get; set; }
        public string TkCo0 { get; set; }
        public DateTime? NgayCt { get; set; }
        public string Kh0 { get; set; }
        public string NoiDung { get; set; }
        public DateTime? NgayGh { get; set; }
        public string Dktt { get; set; }
        public string Dkgh { get; set; }
        public string Thhl { get; set; }
        public int? Vthhtp { get; set; }
        public decimal? SoLuong { get; set; }
        public decimal? DonGia0 { get; set; }
        public decimal? NgoaiTe0 { get; set; }
        public decimal? Vnd0 { get; set; }
        public decimal? Vnd1 { get; set; }
        public decimal? ThueSuatVat { get; set; }
        public string NguoiPhuTrach { get; set; }
        public decimal? TyGia { get; set; }
        public int? LoaiTien { get; set; }
        public decimal? SumVnd0 { get; set; }
        public decimal? SumVnd1 { get; set; }
        public decimal? SumVnd2 { get; set; }
        public decimal? SumVnd3 { get; set; }
        public decimal? SumVnd4 { get; set; }
        public decimal? SumVnd5 { get; set; }
        public string MaBgBh { get; set; }
        public string Attachment { get; set; }
        public string OngBa { get; set; }
        public int? BoPhan { get; set; }
        public string KhNguoiLh { get; set; }
        public string KhVt { get; set; }
        public int? Stt { get; set; }
        public string DienGiai { get; set; }
        public decimal? DonGiaNt { get; set; }
        public int? VthhtpDvt2 { get; set; }
        public int? SoLuongDv2 { get; set; }
        public decimal? DonGiaDv2 { get; set; }
        public string GhiChu { get; set; }
        public string VthhtpYc { get; set; }
        public int? Dvt { get; set; }
        public string MaVthhtpYc { get; set; }
        public string TtVthhtpYc { get; set; }
        public decimal? TheTich { get; set; }
        public decimal? TongTheTich { get; set; }
        public decimal? Vnd3 { get; set; }
        public byte? ThueSuatXnk { get; set; }
        public int? Kho { get; set; }
        public string TkNo0 { get; set; }
        public string MauHd { get; set; }
        public string KyHieuHd { get; set; }
        public string SeriHd { get; set; }
        public DateTime? NgayHd { get; set; }
        public string OrderCode { get; set; }
        public int? SlOd { get; set; }
        public string TkNo1 { get; set; }
        public string TkCo1 { get; set; }
        public string MaLsx { get; set; }
        public int? Status { get; set; }
    }
}
