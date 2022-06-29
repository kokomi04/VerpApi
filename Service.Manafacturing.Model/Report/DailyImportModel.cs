using System.Collections.Generic;
namespace VErp.Services.Manafacturing.Model.Report
{
    public class DailyImportModel
    {
        public List<DailyProductionHistoryModel> ProductionHistories { get; set; }
        public DailyHumanResourceModel HumanResources { get; set; }
        public List<int> WorkDepartments { get; set; }
        public DailyImportModel()
        {
            ProductionHistories = new List<DailyProductionHistoryModel>();
            WorkDepartments = new List<int>();
        }
    }


    public class DailyProductionHistoryModel
    {
        public int ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
        public decimal OfficeQuantity { get; set; }
        public decimal OvertimeQuantity { get; set; }
    }

    public class DailyHumanResourceModel
    {
        public decimal OfficeWorkDay { get; set; }
        public decimal OvertimeWorkDay { get; set; }
    }
}
