using System.Collections.Generic;

namespace VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill
{
    public class BillInfoModel
    {
        public NonCamelCaseDictionary Info { get; set; }
        public IList<NonCamelCaseDictionary> Rows { get; set; }
        public OutsideImportMappingData OutsideImportMappingData { get; set; }

        private Dictionary<NonCamelCaseDictionary, int> _excelRowNumbers;

        public void SetExcelRowNumbers(Dictionary<NonCamelCaseDictionary, int> excelRowNumbers)
        {
            _excelRowNumbers = excelRowNumbers;
        }

        public Dictionary<NonCamelCaseDictionary, int> GetExcelRowNumbers()
        {
            return _excelRowNumbers;
        }
    }

    public class OutsideImportMappingData
    {
        public string MappingFunctionKey { get; set; }
        public string ObjectId { get; set; }
    }



}
