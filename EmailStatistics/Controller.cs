using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;

namespace EmailStatistics
{
    public class Controller : IController
    {
        // Used in Windows Froms apps if has different ApplicaitonContext than View
        public event EventHandler AppExit;

        IModel _model;
        IView _view;
        IEmailObjectFactory _helperFactory;
   
        // TODO: List of workers so can have multiple
        IEmailWorker _emailWorker;

        public Controller(IModel model, IView view, IEmailObjectFactory factory)
        {
            _model = model;

            _view = view;
            _view.Closed += new EventHandler(view_Closed);
            _view.SetController(this);
            _view.SetModel(_model);

            _helperFactory = factory;

            _view.ShowView();
        }

        #region Event methods

        void view_Closed(object sender, EventArgs e)
        {
            if (AppExit != null)
                AppExit(this, e);
        }

        #endregion

        #region Worker methods

	private void createWorker(ConnectionType conType)
	{
            _emailWorker = _helperFactory.GetWorker(conType);
            _emailWorker.ConnectComplete += new TaskCompleteDelegate(_emailWorker_ConnectComplete);
            _emailWorker.DisconnectComplete += new TaskCompleteDelegate(_emailWorker_DisconnectComplete);
            _emailWorker.GetDataComplete += new TaskCompleteDelegate(_emailWorker_GetDataComplete);
            _emailWorker.NewMail += new NewMailEvent(_emailWorker_NewMail);

            // TODO: Maybe worker should have some king of model, this is stupid
            _emailWorker.InboxCount += new NewModelCount(_emailWorker_InboxCount);
            _emailWorker.SentCount += new NewModelCount(_emailWorker_SentCount);
            _emailWorker.SelectedCount += new NewModelCount(_emailWorker_SelectedCount);
	}

        void _emailWorker_ConnectComplete(bool success, string message)
        {
            if (success)
                _view.SetConnectionOK();
            else
                _view.SetConnectionError(message);
        }

        void _emailWorker_DisconnectComplete(bool success, string message)
        {
            if (success)
                _view.SetDisconnectOK();
            else
                _view.SetDisconnectError(message);
        }

        void _emailWorker_NewMail(Mail newMail)
        {
            //TODO: Check if better to have thread for this
            _model.AddMail(newMail);
        }

        void _emailWorker_GetDataComplete(bool success, string message)
        {
            _model.DataReady();
            _view.DataReady();
        }

        void _emailWorker_SelectedCount(int count)
        {
            _model.SelectedCount = count;
        }

        void _emailWorker_SentCount(int count)
        {
            _model.SentCount = count;
        }

        void _emailWorker_InboxCount(int count)
        {
            _model.InboxCount = count;
        }

        #endregion

        public void GetData(string subject, bool getInbox, bool getSent)
        {
            // Get selected messages count
            _emailWorker.GetSelectedMessageCount(subject, getInbox, getSent);

            _emailWorker.GetData(subject, getInbox, getSent);
        }

        public void SetConfig(ConnectionType conType, string host, int port, bool useSSL, string username, string password)
        {
		// Get new Worker by conType
            // For now only use IMAP
            createWorker(conType);
            _emailWorker.SetConfig(host, port, useSSL, username, password);
        }

        public void EnableConnection()
        {
            _emailWorker.EnableConnection();
        }

        public void DisableConnection()
        {
            _emailWorker.DisableConnection();
        }

        public void Stop()
        {
            _emailWorker.Stop();
        }

    }
}
