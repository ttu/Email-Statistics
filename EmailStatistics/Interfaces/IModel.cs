using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailStatistics
{
    public interface IModel
    {
        event EventHandler ModelUpdated;
        event EventHandler CountersUpdated;

        Dictionary<StatType, int[]> DateStats { get; }
        Dictionary<string, int> UserStats { get; }
        int InboxCount { get; set; }
        int SentCount { get; set; }
        int SelectedCount { get; set; }
        int ProcessedCount { get; }
        int Process { get; }

        bool AddMail(Mail mail);
        bool AddMails(List<Mail> mails);
        string[] GetStatsHeaders(StatType statType);
        void DataReady();
        void Reset();
    }
}
