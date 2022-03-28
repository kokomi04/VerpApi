﻿using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Constants;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;

namespace VErp.Commons.Library
{
    public static class EvalUtils
    {

        public static decimal Eval(string expression)
        {
            try
            {
                var outPut = new NCalc.Expression(expression).Evaluate();
                var result = Convert.ToDecimal(outPut);
                return result;
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ProductErrorCode.InvalidUnitConversionExpression, ProductErrorCode.InvalidUnitConversionExpression.GetEnumDescription() + " => " + expression + " " + ex.Message);
            }
        }

        public static decimal EvalPrimaryQuantityFromProductUnitConversionQuantity(decimal productUnitConversionQuantity, string factorExpression)
        {
            var expression = $"({productUnitConversionQuantity})/({factorExpression})";
            return Eval(expression);
        }

        /*
        public static (bool, decimal) GetPrimaryQuantityFromProductUnitConversionQuantity_bak(decimal productUnitConversionQuantity, decimal factorExpression, decimal inputData, int round)
        {
            var value = (productUnitConversionQuantity / factorExpression).RoundBy(round);
            if (Math.Abs(value - inputData) <= Numbers.INPUT_RATE_STANDARD_ERROR)
            {
                return (true, inputData.RoundBy(round));
            }

            if (inputData == 0)
            {
                return (true, value.RoundBy(round));
            }
            else
            {
                return (false, value.RoundBy(round));
            }
        }

        public static (bool, decimal) GetProductUnitConversionQuantityFromPrimaryQuantity_bak(decimal primaryQuantity, string factorExpression, decimal inputData, int round)
        {
            var expression = $"({primaryQuantity})*({factorExpression})";
            var value = Eval(expression).RoundBy(round);
            if (Math.Abs(value - inputData) <= Numbers.INPUT_RATE_STANDARD_ERROR)
            {
                return (true, inputData.RoundBy(round));
            }

            if (inputData == 0)
            {
                return (true, value.RoundBy(round));
            }
            else
            {
                return (false, value.RoundBy(round));
            }

        }

        public static (bool, decimal) GetProductUnitConversionQuantityFromPrimaryQuantity_bak(decimal primaryQuantity, decimal factorExpression, decimal inputData, int round)
        {
            var value = (primaryQuantity * factorExpression).RoundBy(round);
            if (Math.Abs(value - inputData) <= Numbers.INPUT_RATE_STANDARD_ERROR)
            {
                return (true, inputData.RoundBy(round));
            }

            if (inputData == 0)
            {
                return (true, value.RoundBy(round));
            }
            else
            {
                return (false, value.RoundBy(round));
            }

        }*/

        public static (bool result, decimal primaryQuantity, decimal puQuantity) GetProductUnitConversionQuantityFromPrimaryQuantity(QuantityPairInputModel input)
        {

            decimal calcPuQuantity;
            if (input.FactorExpressionRate > 0)
            {
                calcPuQuantity = (input.PrimaryQuantity * input.FactorExpressionRate.Value).RoundBy(input.PuDecimalPlace);
            }
            else
            {
                calcPuQuantity = Eval($"({input.PrimaryQuantity})*({input.FactorExpression})").RoundBy(input.PuDecimalPlace);
            }

            if (Math.Abs(calcPuQuantity - input.PuQuantity) <= Numbers.INPUT_RATE_STANDARD_ERROR)
            {
                return (true, input.PrimaryQuantity, input.PuQuantity);
            }

            if (input.PuQuantity == 0)
            {
                return (true, input.PrimaryQuantity, calcPuQuantity);
            }


            decimal calcPrimaryQuantity;

            if (input.FactorExpressionRate > 0)
            {
                calcPrimaryQuantity = (input.PuQuantity / input.FactorExpressionRate.Value).RoundBy(input.PrimaryDecimalPlace);
            }
            else
            {
                calcPrimaryQuantity = Eval($"({input.PuQuantity})/({input.FactorExpression})").RoundBy(input.PrimaryDecimalPlace);
            }

            if (Math.Abs(calcPrimaryQuantity - input.PrimaryQuantity) <= Numbers.INPUT_RATE_STANDARD_ERROR)
            {
                return (true, input.PrimaryQuantity, input.PuQuantity);
            }

            if (input.PrimaryQuantity == 0)
            {
                return (true, calcPrimaryQuantity, input.PuQuantity);
            }


            return (false, input.PrimaryQuantity, calcPuQuantity);

        }

    }
}
