using System;

namespace EmailStatistics
{
    public delegate void NewMailEvent(Mail newMail);
    public delegate void NewModelCount(int cout);
    public delegate void TaskCompleteDelegate(bool success, string message);

    public interface IEmailWorker
    {
        event TaskCompleteDelegate ConnectComplete;
        event TaskCompleteDelegate GetDataComplete;
        event TaskCompleteDelegate DisconnectComplete;
        event NewModelCount InboxCount;
        event NewMailEvent NewMail;
        event NewModelCount SelectedCount;
        event NewModelCount SentCount;

        void DisableConnection();
        void EnableConnection();
        void GetData(string subject, bool getInbox, bool getSent);
        
        void GetInboxMessageCount();
        void GetSelectedMessageCount(string subject, bool getInbox, bool getSent);
        void GetSentMessageCount();
       
        void SetConfig(string host, int port, bool useSSL, string username, string password);
        void Stop();
    }
}
