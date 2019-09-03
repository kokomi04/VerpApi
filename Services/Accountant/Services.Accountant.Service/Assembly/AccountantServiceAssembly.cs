using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Services.Accountant.Service
{
    public static class AccountantServiceAssembly
    {
        public static Assembly Assembly => typeof(AccountantServiceAssembly).Assembly;
    }
}
