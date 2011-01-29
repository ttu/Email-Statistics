using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailStatistics
{
    public interface IMail
    {
        DateTime Date { get; set; }

        string From { get; set; }
        List<string> To { get; set; }
        List<string> Cc { get; set; }

        string Subject { get; set; }
        string Body { get; set; }
    }
}
