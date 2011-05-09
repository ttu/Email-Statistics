using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailStatistics
{
    public interface IController
    {
        event EventHandler AppExit;

        void SetConfig(ConnectionType conType, string host, int port, bool useSSL, string username, string password);
        void EnableConnection();
        void DisableConnection();
        void Stop();
        void GetData(string subject, bool getInbox, bool getSent);
        void GetDataNoUpdates(string subject, bool getInbox, bool getSent);
    }
}
