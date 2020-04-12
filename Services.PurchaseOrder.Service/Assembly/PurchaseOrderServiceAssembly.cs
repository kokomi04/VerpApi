using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Services.PurchaseOrder.Service
{
    public static class PurchaseOrderServiceAssembly
    {
        public static Assembly Assembly => typeof(PurchaseOrderServiceAssembly).Assembly;
    }
}
