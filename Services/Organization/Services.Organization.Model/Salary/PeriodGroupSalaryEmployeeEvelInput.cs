using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Organization.Model.Salary
{
    public class SortedSalaryFields
    {
        public IList<SalaryFieldModel> SalariesFields { get; private set; }
        public SortedSalaryFields(IList<SalaryFieldModel> salariesFields)
        {
            SalariesFields = SortFieldNameByReference(salariesFields);
        }

        private IList<SalaryFieldModel> SortFieldNameByReference(IList<SalaryFieldModel> fields)
        {
            var sortedFields = new List<SalaryFieldModel>();

            foreach (var field in fields)
            {
                var stack = new Stack<SalaryFieldModel>();
                stack.Push(field);
                var traveled = new HashSet<SalaryFieldModel>();
                while (stack.Count > 0)
                {
                    SalaryFieldModel currentField = stack.Pop();
                    traveled.Add(currentField);

                    var children = fields.Where(f => f != currentField && ContainRefField(currentField, "#" + f.SalaryFieldName)).ToList();
                    if (children.Count == 0 || children.All(c => sortedFields.Contains(c)))
                    {
                        if (!sortedFields.Contains(currentField))
                        {
                            sortedFields.Add(currentField);
                        }
                    }
                    else
                    {
                        stack.Push(currentField);
                        foreach (var c in children)
                        {
                            if (!traveled.Contains(c))
                                stack.Push(c);
                        }
                    }
                }

            }

            return sortedFields;

        }

        private bool ContainRefField(SalaryFieldModel expression, string fieldName)
        {
            if (expression.Expression == null || expression.Expression.Count == 0) return false;
            return expression.Expression.Any(e => ContainRefField(e, fieldName));
        }

        private bool ContainRefField(SalaryFieldExpressionModel expression, string fieldName)
        {
            return ContainRefField(expression.Filter, fieldName) || ContainVarible(expression.ValueExpression, fieldName);
        }

        private bool ContainRefField(Clause clause, string fieldName)
        {
            if (clause == null) return false;
            if (clause is SingleClause single)
            {
                if (ContainVarible(single.FieldName, fieldName)) return true;
                if (ContainVarible(single.Value?.ToString(), fieldName)) return true;
                return false;
            }
            else
            {
                var arrClause = clause as ArrayClause;
                if (arrClause == null || arrClause.Rules == null || arrClause.Rules.Count == 0) return false;
                return arrClause.Rules.Any(r => ContainRefField(r, fieldName));
            }
        }


        private bool ContainVarible(string str, string childString)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            var _regContainVariable = new Regex($".*(^|[^a-zA-Z0-9_]){childString}([^a-zA-Z0-9_]|$).*");
            return _regContainVariable.IsMatch(str);
        }
    }

    public class PeriodGroupSalaryEmployeeEvelInput : GroupSalaryEmployeeModel
    {
        public SalaryPeriodInfo PeriodInfo { get; set; }
        public SalaryGroupInfo GroupInfo { get; set; }

        public SortedSalaryFields SortedSalaryFields { get; set; }

    }

}
