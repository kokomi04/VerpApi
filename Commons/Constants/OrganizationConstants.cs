namespace VErp.Commons.Constants
{
    public static class OrganizationConstants
    {

        public const double WORKING_HOUR_PER_DAY = 8;

        public const string HR_TABLE_NAME_PREFIX = "_HR";
        public const string HR_TABLE_F_IDENTITY = "F_Id";

        public const string BILL_CODE = "so_ct";
        public const string BILL_DATE = "ngay_ct";


        public const string HR_EMPLOYEE_TYPE_CODE = "CTNS_Ho_So";

        public const string EMPLOYEE_CATEGORY_CODE = "_HO_SO_NHAN_SU";

        public static string GetHrAreaTableName(string hrTypeCode, string hrAreaCode)
        {
            return $"{HR_TABLE_NAME_PREFIX}_{hrTypeCode}_{hrAreaCode}";
        }

        public static string GetEmployeeSalaryTableName(string subsidiaryCode)
        {
            return $"_SalaryEmployee_{subsidiaryCode}";
        }
    }

    public static class EmployeeConstants
    {
        public const string EMPLOYEE_ID = "F_Id";

        public const string EMPLOYEE_CODE = "so_ct";

        public const string TIMEKEEPING_CODE = "ma_cham_cong";

        public const string DEPARTMENT = "bo_phan";
    }
}
