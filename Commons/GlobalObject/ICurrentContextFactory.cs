using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.GlobalObject
{
    public interface ICurrentContextFactory
    {
        void SetCurrentContext(ICurrentContextService currentContext);
        ICurrentContextService GetCurrentContext();
    }
}
