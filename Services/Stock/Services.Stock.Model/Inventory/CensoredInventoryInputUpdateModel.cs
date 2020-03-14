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

        private decimal _OldPrimaryQuantity;
        public decimal OldPrimaryQuantity
        {
            get
            {
                return _OldPrimaryQuantity;
            }
            set
            {
                _OldPrimaryQuantity = value.RelativeTo(NewPrimaryQuantity);               
            }
        }

        private decimal _NewPrimaryQuantity { get; set; }
        public decimal NewPrimaryQuantity
        {
            get
            {
                return _NewPrimaryQuantity;
            }
            set
            {
                _NewPrimaryQuantity = value.RelativeTo(OldPrimaryQuantity);               
            }
        }



        private decimal _OldProductUnitConversionQuantity;
        public decimal OldProductUnitConversionQuantity
        {
            get
            {
                return _OldProductUnitConversionQuantity;
            }
            set
            {
                _OldProductUnitConversionQuantity = value.RelativeTo(NewProductUnitConversionQuantity);
              
            }
        }

        private decimal _NewProductUnitConversionQuantity;
        public decimal NewProductUnitConversionQuantity
        {
            get
            {
                return _NewProductUnitConversionQuantity;
            }
            set
            {
                _NewProductUnitConversionQuantity = value.RelativeTo(OldProductUnitConversionQuantity);
              
            }
        }

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
                     

        private decimal _OldTransferPrimaryQuantity;
        public decimal OldTransferPrimaryQuantity
        {
            get
            {
                return _OldTransferPrimaryQuantity;
            }
            set
            {
                _OldTransferPrimaryQuantity = value.RelativeTo(NewTransferPrimaryQuantity);                
            }
        }

        private decimal _NewTransferPrimaryQuantity { get; set; }
        public decimal NewTransferPrimaryQuantity
        {
            get
            {
                return _NewTransferPrimaryQuantity;
            }
            set
            {
                _NewTransferPrimaryQuantity = value.RelativeTo(OldTransferPrimaryQuantity);                
            }
        }



        private decimal _OldTransferProductUnitConversionQuantity;
        public decimal OldTransferProductUnitConversionQuantity
        {
            get
            {
                return _OldTransferProductUnitConversionQuantity;
            }
            set
            {
                _OldTransferProductUnitConversionQuantity = value.RelativeTo(NewTransferProductUnitConversionQuantity);
               
            }
        }

        private decimal _NewTransferProductUnitConversionQuantity;
        public decimal NewTransferProductUnitConversionQuantity
        {
            get
            {
                return _NewTransferProductUnitConversionQuantity;
            }
            set
            {
                _NewTransferProductUnitConversionQuantity = value.RelativeTo(OldTransferProductUnitConversionQuantity);

            }
        }


    }

}
