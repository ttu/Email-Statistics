using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LumiSoft.Net.IMAP.Client;
using LumiSoft.Net.IMAP;
using LumiSoft.Net;
using System.IO;
using LumiSoft.Net.Mail;
using System.Text.RegularExpressions;

namespace EmailStatistics
{
    public class LumiIMAPGmailHelper : EmailHelper
    {
        IMAP_Client _client;

        private string _inboxName = "INBOX";
        private string _sentFolderName = "[Gmail]/Sent Mail";

        private string _username;
        private string _password;

        private string _subject = "SUBJECT {0}";

        // TODO: Combine patterns to one and move to base class
        // e.g. 2010/6/17 Name <some@address.com>
        // 28. kesäkuuta 2010 11.33 Name <some@address.com>
        private string _gmailEndPattern = @"(20)\d\d/[0-9]{1,2}/[0-9]{1,2}[ ]{1}.*[ ]{1}<[a-zA-Z0-9._%+-]*@[a-zA-Z0-9._%+-]*>" + 
                                            @"|\d{1,2}\.[ ].*<[a-zA-Z0-9._%+-]*@[a-zA-Z0-9._%+-]*>" +
                                             "|[\r\n](> )" +
                                             "|-----Original Message-----";

        // Mails sent from mobiles have different end patterns
        private string _gmailHtmlEnd = "(<div class=\"gmail_quote\">)|(<a href=\"mailto:)";

        /*
         * Might end with:
         * ____________________
         * Date:
         * > Date:
         * WHWHWHW&gt; Date:
         */
        private string _hotmailEndPattern = "[\r\n]([_]{5}|[a-zA-Z]*:|> [a-zA-Z]*:)";

        /*
         * Some had <hr and some <HR
         */
        private string _hotmailHtmlEnd = "<hr|<HR|(&gt; )[a-zA-Z]*:";

        public LumiIMAPGmailHelper()
        {
            _client = new IMAP_Client();
        }

        protected override bool logIn()
        {
            _client.Login(_username, _password);
            return true;
        }

        protected override bool logOut()
        {
            return true;
        }

        public override bool HasConnection()
        {
            return _client.IsAuthenticated;
        }

        public override void SetConfig(string host, int port, bool useSSL, string username, string password)
        {
            _client.Connect(host, port, useSSL);
            _username = username;
            _password = password;
        }

        #region IEnumrable methods

        public override IEnumerable<Mail> GetMails(string subject, bool getInbox, bool getSent)
        {
            if (getInbox)
            {
                _client.SelectFolder(_inboxName);
                int[] id = _client.Search(true, System.Text.Encoding.ASCII.WebName, string.Format(_subject, subject));

                foreach (int i in id)
                {
                    yield return getSingleMail(i);
                }
            }

            if (getSent)
            {
                _client.SelectFolder(_sentFolderName);
                int[] id = _client.Search(true, System.Text.Encoding.ASCII.WebName, string.Format(_subject, subject));

                foreach (int i in id)
                {
                    yield return getSingleMail(i);
                }
            }
        }

        public override IEnumerable<List<Mail>> GetMails(string subject, bool getInbox, bool getSent, int batchSize)
        {
            if (getInbox)
            {
                _client.SelectFolder(_inboxName);

                List<string> uids = getUids(subject, batchSize);
                foreach (string currentUids in uids)
                    yield return getMails(currentUids, true);
            }

            if (getSent)
            {
                _client.SelectFolder(_sentFolderName);

                List<string> uids = getUids(subject, batchSize);
                foreach (string currentUids in uids)
                    yield return getMails(currentUids, true);
            }
        }

        private List<string> getUids(string subject, int batchSize)
        {
            int[] mailUids = _client.Search(true, System.Text.Encoding.ASCII.WebName, string.Format(_subject, subject));

            if (batchSize == 0)
                batchSize = mailUids.Length;

            List<string> idStrings = new List<string>();

            StringBuilder sb = new StringBuilder();

            for (int batchStart = 0; batchStart < mailUids.Length; batchStart += batchSize)
            {
                int loopEnd = batchStart + batchSize < mailUids.Length ? batchStart + batchSize : mailUids.Length;

                for (int i = batchStart; i < loopEnd; i++)
                {
                    sb.Append(mailUids[i].ToString() + ",");
                }

                idStrings.Add(sb.ToString(0, sb.Length - 1));
                sb = new StringBuilder();
            }

            return idStrings;
        }

        private IEnumerable<List<Mail>> proessFolder(string subject, int batchSize)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Message count

        public override int GetMessageCountBySubject(string subject, bool getInbox, bool getSent)
        {
            int count = 0;

            if (getInbox)
            {
                _client.SelectFolder(_inboxName);
                if (string.IsNullOrEmpty(subject))
                    count += _client.SelectedFolder.MessagesCount;
                else
                {
                    int[] id = _client.Search(true, System.Text.Encoding.ASCII.WebName, "SUBJECT " + subject);
                    count += id.Length;
                }
            }

            if (getSent)
            {
                _client.SelectFolder(_sentFolderName);
                if (string.IsNullOrEmpty(subject))
                    count += _client.SelectedFolder.MessagesCount;
                else
                {
                    int[] id = _client.Search(true, System.Text.Encoding.ASCII.WebName, "SUBJECT " + subject);
                    count += id.Length;
                }
            }

            return count;
        }

        #endregion

        #region List<Mail> methods

        public override List<Mail> GetAllInboxMessages()
        {
            return GetMessages(_inboxName);
        }

        public override List<Mail> GetMessages(string folder)
        {
            _client.SelectFolder(folder);

            return getMails("1:*");
        }

        public override List<Mail> GetMessagesBySubject(string subject)
        {
            List<Mail> retVal = getMessagesFromFolder(_inboxName, subject);
            List<Mail> retVal_2 = getMessagesFromFolder(_sentFolderName, subject);

            foreach (Mail m in retVal_2)
                retVal.Add(m);

            return retVal;
        }

        #endregion

        #region Private methods

        private Mail getSingleMail(int uid)
        {
            List<Mail> mails = getMails(uid.ToString(), true);

            Mail retVal = mails.Count > 0 ? mails[0] : null;

            return retVal;
        }

        private List<Mail> getMails(string seqPattern, bool isUids = false)
        {
            List<Mail> retVal = new List<Mail>();

            IMAP_Client_FetchHandler fetchHandler = new IMAP_Client_FetchHandler();

            fetchHandler.Rfc822 += new EventHandler<IMAP_Client_Fetch_Rfc822_EArgs>(delegate(object s, IMAP_Client_Fetch_Rfc822_EArgs e)
            {
                MemoryStream storeStream = new MemoryStream();
                e.Stream = storeStream;
                e.StoringCompleted += new EventHandler(delegate(object s1, EventArgs e1)
                {
                    storeStream.Position = 0;
                    Mail_Message mime = Mail_Message.ParseFromStream(storeStream);
                    // TODO: Date check
                    // if (mime.Date < DateTime.Parse("2011-06-20"))
                        retVal.Add(convertToMail(mime));
                });
            });


            IMAP_SequenceSet seqSet = new IMAP_SequenceSet();
            seqSet.Parse(seqPattern);

            _client.Fetch(
                   isUids,
                   seqSet,
                   new IMAP_Fetch_DataItem[]{
                       new IMAP_Fetch_DataItem_Rfc822()
                    },
                   fetchHandler
               );

            return retVal;
        }

        private List<Mail> getMessagesFromFolder(string folder, string subject)
        {
            _client.SelectFolder(folder);

            string idString = string.Empty;
            bool uids = true;

            if (!string.IsNullOrEmpty(subject))
            {
                int[] id = _client.Search(true, System.Text.Encoding.ASCII.WebName, "SUBJECT " + subject);
                idString = createStringFromIDList(id);
            }
            else
            {
                idString = "1:*";
                uids = false;
            }

            return getMails(idString, uids);
        }

        private string createStringFromIDList(int[] id)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < id.Length - 1; i++)
                sb.Append(id[i] + ",");

            sb.Append(id[id.Length - 1]);

            return sb.ToString();
        }

        private Mail convertToMail(Mail_Message mime)
        {
            Mail retVal = new Mail();
            retVal.Subject = mime.Subject;
            retVal.From = mime.From[0].Address;
            retVal.Date = mime.Date;

            // TODO: Combine patterns
            if (retVal.From.EndsWith("hotmail.com"))
            {
                retVal.Body = clearMailBody(mime.BodyText, _hotmailEndPattern);
                retVal.BodyHtml = clearMailBodyHtml(mime.BodyHtmlText, _hotmailHtmlEnd);                    
            }
            else
            {
                retVal.Body = clearMailBody(mime.BodyText, _gmailEndPattern);
                retVal.BodyHtml = clearMailBodyHtml(mime.BodyHtmlText, _gmailHtmlEnd);    
            }

            return retVal;
        }

        private string clearMailBody(string fullBody, string endPattern)
        {
            if (string.IsNullOrEmpty(fullBody))
                return string.Empty;

            Regex regex = new Regex(endPattern);
            Match match = regex.Match(fullBody);

            int endIndex = match.Success ? match.Index : fullBody.Length;

            string retVal = fullBody.Substring(0, endIndex);
            retVal = retVal.Replace("\r\n", " ");

            return retVal.Trim();
        }

        private string clearMailBodyHtml(string fullBody, string endPattern)
        {
            if (string.IsNullOrEmpty(fullBody))
                return string.Empty;

            Regex regex = new Regex(endPattern);
            Match match = regex.Match(fullBody);

            int endIndex = match.Success ? match.Index : fullBody.Length;

            string retVal = fullBody.Substring(0, endIndex);

            return retVal.Trim();
        }

        #endregion
    }
}
