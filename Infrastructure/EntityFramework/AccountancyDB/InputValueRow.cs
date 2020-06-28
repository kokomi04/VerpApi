using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class InputValueRow
    {
        public long FId { get; set; }
        public int InputTypeId { get; set; }
        public long InputBillFId { get; set; }
        public int BillVersion { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public string MaCt { get; set; }
        public int? StockId { get; set; }
        public string DienGiai { get; set; }
        public DateTime? NgayCt { get; set; }
        public string SoCt { get; set; }
        public string MauHd { get; set; }
        public string SeriHd { get; set; }
        public DateTime? NgayHd { get; set; }
        public string Attachment { get; set; }
        public int? LoaiTien { get; set; }
        public decimal? TyGia { get; set; }
        public int? TkNo0 { get; set; }
        public int? TkCo0 { get; set; }
        public string OngBa { get; set; }
        public string DiaChi { get; set; }
        public int? BoPhan { get; set; }
        public int? MaKh0 { get; set; }
        public string TenKh0 { get; set; }
        public int? MaKhc0 { get; set; }
        public string TenKhc0 { get; set; }
        public decimal? Stt { get; set; }
        public string NoiDung { get; set; }
        public int? MaVthhtp { get; set; }
        public string TenVthhtp { get; set; }
        public decimal? SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        public decimal? NgoaiTe0 { get; set; }
        public decimal? Vnd0 { get; set; }
        public long? ThueSuatVat { get; set; }
        public string GhiChu { get; set; }
        public int? Kho { get; set; }
        public int? KhoC { get; set; }
        public int? KheUocVay { get; set; }
        public int? TkThuKbnn { get; set; }
        public int? MaChuongNsnn { get; set; }
        public int? MaMucNsnn { get; set; }
        public int? CongTrinh { get; set; }
        public int? PhanXuong { get; set; }
        public int? KhoanMucCp { get; set; }
        public int? KhoanMucTc { get; set; }
        public string PoCode { get; set; }
        public string OrderCode { get; set; }
        public string MaLsx { get; set; }
        public string Dvt { get; set; }
        public decimal? ThueSuatXnk { get; set; }
        public decimal? Vnd1 { get; set; }
        public decimal? TongTienHang { get; set; }
        public decimal? SlTonKho { get; set; }
        public decimal? SlPo { get; set; }
        public decimal? SlOd { get; set; }
        public decimal? SlReq { get; set; }
        public decimal? TongCong { get; set; }
        public decimal? TongNgoaiTe0 { get; set; }
        public decimal? TongVnd0 { get; set; }
        public decimal? TongVnd1 { get; set; }
        public int? TkNo1 { get; set; }
        public int? TkCo1 { get; set; }
        public int? TkNo2 { get; set; }
        public int? TkCo2 { get; set; }
        public int? TkNo3 { get; set; }
        public int? TkCo3 { get; set; }
        public int? MaTscd { get; set; }
        public string TenTscd { get; set; }
        public int? MaKh1 { get; set; }
        public string TenKh1 { get; set; }
        public int? MaKh3 { get; set; }
        public string TenKh3 { get; set; }
        public int? TknhDn { get; set; }
        public int? TknhDt { get; set; }
        public string KyHieuHd { get; set; }
        public string MaLinkHd { get; set; }
        public bool? ConstrainTyGia { get; set; }
        public string Dvt2 { get; set; }
        public decimal? SoLuongDv2 { get; set; }
        public decimal? DonGiaDvt2 { get; set; }
        public long? Vnd3 { get; set; }
    }
}
