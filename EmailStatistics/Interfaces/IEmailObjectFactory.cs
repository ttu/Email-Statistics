using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailStatistics
{
    public interface IEmailObjectFactory
    {
        IEmaillHelper GetInterIMAPHelper();
        IEmailWorker GetWorker(ConnectionType connectionType);
    }
}
