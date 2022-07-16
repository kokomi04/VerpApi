using System.ComponentModel;

namespace VErp.Commons.Enums.MasterEnum
{
    public enum EnumOperator
    {
        [AllowedDataType(new EnumDataType[] {
            EnumDataType.BigInt,
            EnumDataType.Boolean,
            EnumDataType.Date,
            EnumDataType.Decimal,
            EnumDataType.Email,
            EnumDataType.Int,
            EnumDataType.Text,
            EnumDataType.Percentage
        })]
        [ParamNumber(1)]
        [Description("Equal")]
        Equal = 1,


        [AllowedDataType(new EnumDataType[] {
            EnumDataType.BigInt,
            EnumDataType.Boolean,
            EnumDataType.Date,
            EnumDataType.Decimal,
            EnumDataType.Email,
            EnumDataType.Int,
            EnumDataType.Percentage,
            EnumDataType.Text
        })]
        [ParamNumber(1)]
        [Description("Is Not Equal")]
        NotEqual = 2,


        [AllowedDataType(new EnumDataType[] {
            EnumDataType.Email,
            EnumDataType.Text
        })]
        [ParamNumber(1)]
        [Description("Contains")]
        Contains = 3,


        [AllowedDataType(new EnumDataType[] {
            EnumDataType.Email,
            EnumDataType.Text
        })]
        [ParamNumber(1)]
        [Description("NotContains")]
        NotContains = 33,


        [AllowedDataType(new EnumDataType[] {
            EnumDataType.Int,
            EnumDataType.Text
        })]
        [ParamNumber(1)]
        [Description("In List")]
        InList = 4,


        [AllowedDataType(new EnumDataType[] {
            EnumDataType.Text
        })]
        [ParamNumber(0)]
        [Description("IsLeafNode")]
        IsLeafNode = 5,


        [AllowedDataType(new EnumDataType[] {
            EnumDataType.Email,
            EnumDataType.Text
        })]
        [ParamNumber(1)]
        [Description("StartsWith")]
        StartsWith = 6,


        [AllowedDataType(new EnumDataType[] {
            EnumDataType.Email,
            EnumDataType.Text
        })]
        [ParamNumber(1)]
        [Description("NotStartsWith")]
        NotStartsWith = 66,


        [AllowedDataType(new EnumDataType[] {
            EnumDataType.Email,
            EnumDataType.Text
        })]
        [ParamNumber(1)]
        [Description("EndsWith")]
        EndsWith = 7,


        [AllowedDataType(new EnumDataType[] {
            EnumDataType.Email,
            EnumDataType.Text
        })]
        [ParamNumber(1)]
        [Description("NotEndsWith")]
        NotEndsWith = 77,


        [AllowedDataType(new EnumDataType[] {
            EnumDataType.BigInt,
            EnumDataType.Date,
            EnumDataType.Decimal,
            EnumDataType.Int,
            EnumDataType.Percentage
        })]
        [ParamNumber(1)]
        [Description("Greater")]
        Greater = 8,


        [AllowedDataType(new EnumDataType[] {
            EnumDataType.BigInt,
            EnumDataType.Date,
            EnumDataType.Decimal,
            EnumDataType.Int,
            EnumDataType.Percentage
        })]
        [ParamNumber(1)]
        [Description("GreaterOrEqual")]
        GreaterOrEqual = 9,


        [AllowedDataType(new EnumDataType[] {
            EnumDataType.BigInt,
            EnumDataType.Date,
            EnumDataType.Decimal,
            EnumDataType.Int,
            EnumDataType.Percentage
        })]
        [ParamNumber(1)]
        [Description("LessThan")]
        LessThan = 10,


        [AllowedDataType(new EnumDataType[] {
            EnumDataType.BigInt,
            EnumDataType.Date,
            EnumDataType.Decimal,
            EnumDataType.Int,
            EnumDataType.Percentage
        })]
        [ParamNumber(1)]
        [Description("LessThanOrEqual")]
        LessThanOrEqual = 11,


        [AllowedDataType(new EnumDataType[] {
            EnumDataType.BigInt,
            EnumDataType.Boolean,
            EnumDataType.Date,
            EnumDataType.Decimal,
            EnumDataType.Email,
            EnumDataType.Int,
            EnumDataType.Text,
            EnumDataType.Percentage
        })]
        [ParamNumber(0)]
        [Description("Is Null")]
        IsNull = 12,


        [AllowedDataType(new EnumDataType[] {
            EnumDataType.Email,
            EnumDataType.Text
        })]
        [ParamNumber(0)]
        [Description("Is Empty")]
        IsEmpty = 13,


        [AllowedDataType(new EnumDataType[] {
            EnumDataType.BigInt,
            EnumDataType.Boolean,
            EnumDataType.Date,
            EnumDataType.Decimal,
            EnumDataType.Email,
            EnumDataType.Int,
            EnumDataType.Text,
            EnumDataType.Percentage
        })]
        [ParamNumber(0)]
        [Description("Is Null Or Empty")]
        IsNullOrEmpty = 14
    }

    public enum EnumLogicOperator
    {
        [Description("AND")]
        And = 1,
        [Description("OR")]
        Or = 2
    }
}
