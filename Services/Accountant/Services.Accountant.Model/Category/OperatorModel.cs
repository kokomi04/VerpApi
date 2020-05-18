namespace VErp.Services.Accountant.Model.Category

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
