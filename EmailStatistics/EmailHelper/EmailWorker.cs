using System;
using System.ComponentModel;
using System.Collections.Generic;
namespace EmailStatistics
{
    // Lets put all this in own class, just in case we want 2 or more some day?
    public class EmailWorker : IEmailWorker
    {
        public event NewMailEvent NewMail;
        public event NewMailsEvent NewMails;

        public event TaskCompleteDelegate ConnectComplete;
        public event TaskCompleteDelegate DisconnectComplete;
        public event TaskCompleteDelegate GetDataComplete;

        public event NewModelCount InboxCount;
        public event NewModelCount SentCount;
        public event NewModelCount SelectedCount;

        IEmaillHelper _helper;

        // TODO: Maybe few more workers is better, changing method with delegate is maybe littel overkill
        BackgroundWorker _emailWorker = new BackgroundWorker();

        public delegate void WorkMethod();

        WorkMethod _emailWorkerWorkMethod;
        WorkMethod _emailWorkerCompleteMethod;
        WorkMethod _emailWorkerProgressMethod;

        private string _subject;
        private bool _getInbox;
        private bool _getSent;

        public EmailWorker(IEmaillHelper helper)
        {
            _helper = helper;

            _emailWorker.DoWork += new DoWorkEventHandler(emailWorker_DoWork);
            _emailWorker.ProgressChanged += new ProgressChangedEventHandler(emailWorker_ProgressChanged);
            _emailWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(emailWorker_RunWorkerCompleted);
            _emailWorker.WorkerReportsProgress = true;
            _emailWorker.WorkerSupportsCancellation = true;
        }

        #region Worker methods

        void emailWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _emailWorkerWorkMethod();
        }

        void emailWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _emailWorkerProgressMethod();
        }

        void emailWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _emailWorkerCompleteMethod();
        }

        #endregion

        public void GetData(string subject, bool getInbox, bool getSent)
        {
            // Save info for later use
            _subject = subject;
            _getInbox = getInbox;
            _getSent = getSent;

            _emailWorkerWorkMethod = getDataInBatchStart;
            _emailWorkerProgressMethod = getDataProgressMailList;
            _emailWorkerCompleteMethod = getDataComplete;
            _emailWorker.RunWorkerAsync();
        }

        public void GetDataNoUpdates(string subject, bool getInbox, bool getSent)
        {
            // Save info for later use
            _subject = subject;
            _getInbox = getInbox;
            _getSent = getSent;

            _emailWorkerWorkMethod = getDataNoUpdatesStart;
            _emailWorkerProgressMethod = getDataProgressSingleMail;
            _emailWorkerCompleteMethod = getDataComplete;
            _emailWorker.RunWorkerAsync();
        }

        public void GetSelectedMessageCount(string subject, bool getInbox, bool getSent)
        {
            // Save info for later use
            _subject = subject;
            _getInbox = getInbox;
            _getSent = getSent;
        }

        public void GetInboxMessageCount()
        {

        }

        public void GetSentMessageCount()
        {

        }

        public void SetConfig(string host, int port, bool useSSL, string username, string password)
        {
            // This is fast, so no need for threading
            _helper.SetConfig(host, port, useSSL, username, password);
        }

        public void EnableConnection()
        {
            _emailWorkerWorkMethod = enableConnectionStart;
            _emailWorkerCompleteMethod = enableConnectionComplete;
            _emailWorker.RunWorkerAsync();
        }

        public void DisableConnection()
        {
            _emailWorkerWorkMethod = disableConnectionStart;
            _emailWorkerCompleteMethod = disableConnectionComplete;
            _emailWorker.RunWorkerAsync();
        }

        public void Stop()
        {
            if (_emailWorker.IsBusy)
                _emailWorker.CancelAsync();
        }

        public bool IsBusy()
        {
            return _emailWorker.IsBusy;
        }


        #region Connection methods

        private void enableConnectionStart()
        {
            _helper.EnableConnection();
        }

        private void enableConnectionComplete()
        {
            if (ConnectComplete == null)
                return;

            if (_helper.HasConnection())
            {
                int result = _helper.GetMessageCountBySubject(string.Empty, true, false);

                if (InboxCount != null)
                    InboxCount(result);

                result = _helper.GetMessageCountBySubject(string.Empty, false, true);

                if (SentCount != null)
                    SentCount(result);

                ConnectComplete(true, string.Empty);
            }
            else
                ConnectComplete(false, "Connection Error");
        }

        private void disableConnectionStart()
        {
            _helper.DisableConnection();
        }

        private void disableConnectionComplete()
        {
            if (DisconnectComplete == null)
                return;

            if (_helper.HasConnection())
                DisconnectComplete(false, "Disconnection Error");
            else
                DisconnectComplete(true, string.Empty);
        }

        #endregion

        #region Data methods

        // Make thread safe
        Stack<Mail> _newMail = new Stack<Mail>();

        // Make thread safe
        Stack<List<Mail>> _newMailList = new Stack<List<Mail>>();

        private void getDataStart()
        {
            int result = _helper.GetMessageCountBySubject(_subject, _getInbox, _getSent);

            // This one will be in wrong thread, but its ok, cos it will not be shown immediately
            if (SelectedCount != null)
                SelectedCount(result);

            foreach (Mail mail in _helper.GetMails(_subject, _getInbox, _getSent))
            {
                _newMail.Push(mail);
                _emailWorker.ReportProgress(1);
            }
        }

        private void getDataInBatchStart()
        {
            int result = _helper.GetMessageCountBySubject(_subject, _getInbox, _getSent);

            // This one will be in wrong thread, but its ok, cos it will not be shown immediately
            if (SelectedCount != null)
                SelectedCount(result);

            // Count batch size
            int updatePercent = 5;
            int batchSize = result / 100 * (updatePercent + 1);

            foreach (List<Mail> mail in _helper.GetMails(_subject, _getInbox, _getSent, batchSize))
            {
                _newMailList.Push(mail);
                _emailWorker.ReportProgress(1);
            }
        }

        private void getDataNoUpdatesStart()
        {
            int result = _helper.GetMessageCountBySubject(_subject, _getInbox, _getSent);

            // This one will be in wrong thread, but its ok, cos it will not be shown immediately
            if (SelectedCount != null)
                SelectedCount(result);

            List<Mail> mails = _helper.GetMessagesBySubject(_subject);

            foreach (Mail mail in mails)
                _newMail.Push(mail);

            _emailWorker.ReportProgress(1);
        }

        private void getDataProgressSingleMail()
        {
            if (NewMail == null)
                return;

            while (_newMail.Count > 0)
            {
                NewMail(_newMail.Pop());
            }
        }

        private void getDataProgressMailList()
        {
            if (NewMails == null)
                return;

            while (_newMailList.Count > 0)
            {
                NewMails(_newMailList.Pop());
            }
        }

        private void getDataComplete()
        {
            if (GetDataComplete == null)
                return;

            if (!_helper.HasConnection())
                // Something went wrong
                GetDataComplete(false, "Something wrong with connection");
            else
                // All ready in the kingdom
                GetDataComplete(true, string.Empty);
        }

        #endregion

    }
}

