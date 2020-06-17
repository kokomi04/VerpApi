using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Services.Accountancy.Service
{
    public static class AccountancyServiceAssembly
    {
        public static Assembly Assembly => typeof(AccountancyServiceAssembly).Assembly;
    }
}
