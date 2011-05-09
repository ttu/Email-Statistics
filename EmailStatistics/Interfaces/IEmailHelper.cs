using System;
using System.Collections.Generic;

namespace EmailStatistics
{
    public interface IEmaillHelper
    {
        bool HasConnection();

        void EnableConnection();
        void DisableConnection();
        void SetConfig(string host, int port, bool useSSL, string username, string password);

        IEnumerable<Mail> GetMails(string subject, bool getInbox, bool getSent);
        IEnumerable<List<Mail>> GetMails(string subject, bool getInbox, bool getSent, int batchSize);
        List<Mail> GetMessagesBySubject(string subject);
        int GetMessageCountBySubject(string subject, bool getInbox, bool getSent);
    }

    public enum ConnectionType
    {
        IMAP,
        POP
    }

    public enum StatType
    {
        User,
        //Year,
        Month,
        //Week,
        Day,
        //Hour12,
        //Hour6,
        Hour4,
        //Hour2,
        Hour1
    }
}
