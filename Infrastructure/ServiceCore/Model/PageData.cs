using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;

namespace VErp.Infrastructure.ServiceCore.Model
{
    public class PageData<T>
    {
        public int Total { get; set; }
        public IList<T> List { get; set; }

        public object AdditionResult { get; set; }

        public static implicit operator PageData<T>((IList<T> list, int total, object additionResult) result)
        {
            return new PageData<T>()
            {
                Total = result.total,
                List = result.list,
                AdditionResult = result.additionResult
            };
        }

        public static implicit operator PageData<T>((IList<T> list, int total) result)
        {
            return new PageData<T>()
            {
                Total = result.total,
                List = result.list
            };
        }
    }

    public class PageDataTable
    {
        public long Total { get; set; }
        public IList<NonCamelCaseDictionary> List { get; set; }

        //public static implicit operator PageDataTable((DataTable list, int total) result)
        //{
        //    return (result.list, result.total);
        //}

        public static implicit operator PageDataTable((DataTable list, long total) result)
        {
            var lst = new List<NonCamelCaseDictionary>();

            for (var i = 0; i < result.list.Rows.Count; i++)
            {
                var row = result.list.Rows[i];
                var dic = new NonCamelCaseDictionary();
                foreach (DataColumn c in result.list.Columns)
                {
                    var v = row[c];
                    if (v != null && v.GetType() == typeof(DateTime) || v.GetType() == typeof(DateTime?))
                    {
                        var vInDateTime = (v as DateTime?).GetUnix();
                        dic.Add(c.ColumnName, vInDateTime);
                    }
                    else
                    {
                        dic.Add(c.ColumnName, row[c]);
                    }
                }
                lst.Add(dic);
            }
            return new PageDataTable()
            {
                Total = result.total,
                List = lst
            };
        }
    }
}
