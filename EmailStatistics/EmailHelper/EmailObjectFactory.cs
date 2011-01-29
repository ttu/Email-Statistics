using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailStatistics
{
    public class EmailObjectFactory : IEmailObjectFactory
    {
        #region IGmailHelperFactory Members

        public IEmaillHelper GetInterIMAPHelper()
        {
            return new InterIMAPGmailHelper();
        }

        public IEmaillHelper GetOpenPOPGmailHelper()
        {
            return new InterIMAPGmailHelper();

        }

        public IEmailWorker GetWorker(ConnectionType connectionType)
        {
            IEmaillHelper helper = null;

            switch (connectionType)
            {
                case ConnectionType.IMAP:
                    helper = GetInterIMAPHelper();
                    break;
                case ConnectionType.POP:
                    helper = GetOpenPOPGmailHelper();
                    break;
                default:
                    helper = GetInterIMAPHelper();
                    break;
            }
            return new EmailWorker(helper);
        }

        #endregion
    }
}
