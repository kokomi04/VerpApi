namespace VErp.Commons.Enums.Organization.TimeKeeping
{
    public enum EnumTimeSortType
    {
        Automation = 1, // Sắp xếp theo giờ vào, giờ ra tự động
        SplitHour = 2, // Chọn giờ vào giờ ra theo khoảng phân giờ
        System = 3, // Chọn giờ vào giờ ra theo số máy chấm công
        FirstOnLastOut = 4, //Giờ vào là giờ đầu tiền, giờ ra là giờ cuối cùng trong ngày
        CheckinCheckOut = 5 // Theo checkin, checkout trên máy chấm công
    }
}