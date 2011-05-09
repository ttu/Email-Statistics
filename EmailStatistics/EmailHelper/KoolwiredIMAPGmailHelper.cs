using System;
using Koolwired.Imap;
using System.Collections.Generic;

namespace EmailStatistics
{
    // TODO: Whole class
	public class KoolwiredIMAPGmailHelper : EmailHelper
	{
		ImapConnect _connection;
		ImapCommand _command;
		ImapAuthenticate _authenticate;

        private string _inboxName = "INBOX";
        private string _sentFolderName = "[Gmail]";
        private string _sentSubFolderName = "Sent Mail";

		public KoolwiredIMAPGmailHelper ():base()
		{
			
		}
		
		protected override bool logIn()
		{
			_connection.Open();
			_authenticate.Login();

            return true;
		}

        protected override bool logOut()
		{
			_authenticate.Logout();
			_connection.Close();

            return true;
		}
		
		#region override and new example 
		
		protected override void DoSomething ()
		{
			DoSomethingElese();
			
			base.DoSomething();
		}
		
		new protected void DoSomethingElese()
		{
			base.DoSomethingElese();
		}
		
		#endregion
		
		#region implemented abstract members of EmailStatistics.EmailHelper
		
		public override void SetConfig(string host, int port, bool useSSL, string username, string password)
		{
            _connection = new ImapConnect(host, port, useSSL);
            _command = new ImapCommand(_connection);
            _authenticate = new ImapAuthenticate(_connection, username, password);
		}
		
		public override List<Mail> GetAllInboxMessages ()
		{
			throw new System.NotImplementedException();
		}
				
		public override List<Mail> GetMessages (string folder)
		{
			throw new System.NotImplementedException();
		}
			
		public override List<Mail> GetMessagesBySubject (string subject)
		{
			logIn();
			
			ImapMailbox mailbox = _command.Select("INBOX");
            int totalRecords = mailbox.Exist;
            mailbox = _command.Fetch(mailbox);
			
			logOut();
			
			return null;
		}

        public override IEnumerable<Mail> GetMails(string subject, bool getInbox, bool getSent)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<List<Mail>> GetMails(string subject, bool getInbox, bool getSent, int batchSize)
        {
            throw new NotImplementedException();
        }

        public override bool HasConnection()
        {
            throw new NotImplementedException();
        }

        public override int GetMessageCountBySubject(string subject, bool getInbox, bool getSent)
        {
            throw new NotImplementedException();
        }
		
		#endregion

      
    }
}

