using System;
using System.Collections.Generic;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;

namespace VErp.Services.Stock.Model.Inventory
{   

    public class CensoredInventoryInputProducts
    {
        public long InventoryDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public int PrimaryUnitId { get; set; }
        public decimal OldPrimaryQuantity { get; set; }
        public decimal NewPrimaryQuantity { get; set; }
        public int ProductUnitConversionId { get; set; }
        public string ProductUnitConversionName { get; set; }
        public string FactorExpression { get; set; }
        public decimal OldProductUnitConversionQuantity { get; set; }
        public decimal NewProductUnitConversionQuantity { get; set; }
        public long ToPackageId { get; set; }

        public IList<CensoredInventoryInputObject> AffectObjects { get; set; }

    }

    public class CensoredInventoryInputObject
    {
        public string ObjectKey
        {
            get
            {
                return Utils.GetObjectKey(ObjectTypeId, ObjectId);
            }
        }
        public bool IsRoot { get; set; }
        public bool IsCurrentFlow { get; set; }
        public long ObjectId { get; set; }
        public string ObjectCode { get; set; }
        public EnumObjectType ObjectTypeId { get; set; }

        public decimal OldPrimaryQuantity { get; set; }
        public decimal NewPrimaryQuantity { get; set; }

        public decimal OldProductUnitConversionQuantity { get; set; }
        public decimal NewProductUnitConversionQuantity { get; set; }

        public IList<TransferToObject> Children { get; set; }
    }


    public class TransferToObject
    {
        public string ObjectKey
        {
            get
            {
                return Utils.GetObjectKey(ObjectTypeId, ObjectId);
            }
        }
        public bool IsEditable { get; set; }
        public long ObjectId { get; set; }
        public EnumObjectType ObjectTypeId { get; set; }
        public EnumPackageOperationType PackageOperationTypeId { get; set; }

        public decimal OldTransferPrimaryQuantity { get; set; }
        public decimal NewTransferPrimaryQuantity { get; set; }

        public decimal OldTransferProductUnitConversionQuantity { get; set; }
        public decimal NewTransferProductUnitConversionQuantity { get; set; }
    }

}
