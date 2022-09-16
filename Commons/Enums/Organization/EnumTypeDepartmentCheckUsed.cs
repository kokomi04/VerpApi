namespace VErp.Commons.Enums.Organization
{
    public enum EnumTypeDepartmentCheckUsed
    {
        //Kiểm tra bộ phận chuyên trách từ thông tin phân công
        Assignment = 1,
        //Kiểm tra nhà máy  từ thông tin LSX
        ProductionOrder = 2,
        //Kiểm tra bộ phận chuyên trách từ thông tin công đoạn
        Step = 3,
        //Kiểm tra cả phân công và công đoạn
        AssignmentAndStep = 4,
        //Kiểm tra cả phân công, công đoạn và LSX
        All = 5,

    }
}
