namespace VErp.Commons.Enums.Organization
{
    public enum EnumTypeDepartmentCheckUsed
    {
        //Kiểm tra bộ phận chuyên trách từ thông tin phân công
        Assignment = 1,
        //Kiểm tra nhà máy  từ thông tin LSX
        ProductionOrder = 2,
        //Kiểm tra cả phân công và LSX
        AssignmentAndProductionOrder = 3,
    }
}
