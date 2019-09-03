using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.ApiCore.Model
{
    public class VerpException : Exception
    {
        public VerpException()
        { }

        public VerpException(string message)
            : base(message)
        { }

        public VerpException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
