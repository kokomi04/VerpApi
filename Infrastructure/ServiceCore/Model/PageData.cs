using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.ServiceCore.Model
{
    public class PageData<T>
    {
        public int Total { get; set; }
        public IList<T> List { get; set; }

        public static implicit operator PageData<T>((IList<T> list, int total) result)
        {
            return new PageData<T>()
            {
                Total = result.total,
                List = result.list
            };
        }
    }
}
