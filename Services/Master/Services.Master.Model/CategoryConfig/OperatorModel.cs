namespace VErp.Services.Master.Model.CategoryConfig
{
    public class OperatorModel: LogicOperatorModel
    {
        public int ParamNumber { get; set; }
    }

    public class LogicOperatorModel 
    {
        public int Value { get; set; }
        public string Title { get; set; }
    }
}
