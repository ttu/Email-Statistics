using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace EmailStatistics
{
    // This class connects to Email and does all the funky stuff
    public abstract class EmailHelper : IEmaillHelper
    {
        protected int _id;

        public EmailHelper()
        {

        }

        #region override example

        // This function can be overridden in child classes
        protected virtual void DoSomething()
        {
            // Do something in here	
        }

        // This can not be overridden, but only hidden by using new
        // then which method is called depends on which type is used (base or inherited class)
        protected void DoSomethingElese()
        {
            // Do something else
        }

        #endregion

        #region Public methods

        public int[] GetDateStatistics<T>(StatType statType, List<T> elements, string datePropertyName)
        {
            List<StatType> stats = new List<StatType>();
            stats.Add(statType);

            Dictionary<StatType, int[]> retVal = GetDateStatistics<T>(stats, elements, datePropertyName);

            return retVal[statType];
        }

        public Dictionary<StatType, int[]> GetDateStatistics<T>(List<StatType> statTypes, List<T> elements, string datePropertyName)
        {
            Dictionary<StatType, int[]> stats = new Dictionary<StatType, int[]>();

            foreach (StatType st in statTypes)
            {
                int[] countArray = createStatsArray(st);
                stats.Add(st, countArray);
            }

            if (elements.Count == 0)
                return stats;

            Type type = typeof(T);

            PropertyInfo pi = type.GetProperty(datePropertyName);

            foreach (T e in elements)
            {
                try
                {
                    DateTime dt = (DateTime)pi.GetValue(e, null);

                    foreach (StatType st in statTypes)
                        insertToStatsCollection(st, stats, dt);
                }
                catch (Exception)
                { /* Throw away this sample */ }

            }

            return stats;
        }

        public Dictionary<string, int> GetUserSatistics<T, K>(List<T> elements, string fromProperty, string addressProperty)
        {
            Dictionary<string, int> stats = new Dictionary<string, int>();

            if (elements.Count == 0)
                return stats;

            Type type = typeof(T);

            PropertyInfo pi = type.GetProperty(fromProperty);

            Type addressType = typeof(K);
            PropertyInfo piFrom = addressType.GetProperty(addressProperty);

            foreach (T e in elements)
            {
                try
                {
                    // TODO: Make better type checking
                    // For now lets just assyme taht if namespace is Generic it's a list, else just xxMAilAddress
                    if (pi.PropertyType.Namespace.Equals("System.Collections.Generic"))
                    {
                        List<K> addressList = (List<K>)pi.GetValue(e, null);

                        foreach (K address in addressList)
                        {
                            updateStats<K>(stats, piFrom, address);
                        }
                    }
                    else
                    {
                        K address = (K)pi.GetValue(e, null);

                        updateStats<K>(stats, piFrom, address);
                    }
                }
                catch (Exception)
                { /* Throw away this sample */ }

            }

            return stats;
        }

        #endregion

        #region Virtual methods

        public virtual void EnableConnection()
        {
            logIn();
        }

        public virtual void DisableConnection()
        {
            logOut();
        }

        #endregion

        #region Abstract methods

        protected abstract bool logIn();
        protected abstract bool logOut();
        public abstract bool HasConnection();
        public abstract void SetConfig(string host, int port, bool useSSL, string username, string password);

        public abstract IEnumerable<Mail> GetMails(string subject, bool getInbox, bool getSent);
        public abstract int GetMessageCountBySubject(string subject, bool getInbox, bool getSent);

        // Not required by interface
        public abstract List<Mail> GetAllInboxMessages();
        public abstract List<Mail> GetMessages(string folder);
        public abstract List<Mail> GetMessagesBySubject(string subject);

        #endregion

        #region Private methods

        private int[] createStatsArray(StatType statType)
        {
            switch (statType)
            {
                case StatType.Month:
                    return new int[12];
                case StatType.Day:
                    return new int[7];
                case StatType.Hour4:
                    return new int[6];
                case StatType.Hour1:
                    return new int[24];
                default:
                    return new int[1];
            }
        }

        private void insertToStatsCollection(StatType statType, Dictionary<StatType, int[]> stats, DateTime dt)
        {
            int index = 0;

            switch (statType)
            {
                case StatType.Month:
                    index = dt.Month - 1;
                    break;
                case StatType.Day:
                    index = (int)dt.DayOfWeek; //sunday is first, damn yanks
                    break;
                case StatType.Hour4:
                    index = dt.Hour / 4;
                    break;
                case StatType.Hour1:
                    index = dt.Hour;
                    break;
                default:
                    break;
            }

            stats[statType][index]++;
        }

        private static void updateStats<K>(Dictionary<string, int> stats, PropertyInfo piFrom, K address)
        {
            string from = (string)piFrom.GetValue(address, null);

            if (stats.ContainsKey(from))
                stats[from]++;
            else
                stats.Add(from, 1);
        }

        #endregion

        #region Dictionary combine helpers

        protected Dictionary<string, int> CombineDictionarys(Dictionary<string, int> dict_1, Dictionary<string, int> dict_2)
        {
            Dictionary<string, int> retVal = dict_1;

            foreach (KeyValuePair<string, int> kvp in dict_2)
            {
                if (retVal.ContainsKey(kvp.Key))
                    retVal[kvp.Key] += kvp.Value;
                else
                    retVal.Add(kvp.Key, kvp.Value);
            }

            return retVal;
        }

        protected Dictionary<StatType, int[]> CombineDictionarys(Dictionary<StatType, int[]> dict_1, Dictionary<StatType, int[]> dict_2)
        {
            Dictionary<StatType, int[]> retVal = dict_1;

            foreach (KeyValuePair<StatType, int[]> kvp in dict_2)
            {
                if (retVal.ContainsKey(kvp.Key))
                {
                    for (int i = 0; i < retVal[kvp.Key].Length; i++)
                    {
                        retVal[kvp.Key][i] += kvp.Value[i];
                    }
                }
                else
                    retVal.Add(kvp.Key, kvp.Value);
            }

            return retVal;
        }

        #endregion
    }

    public class Mail : IMail
    {
        public DateTime Date { get; set; }

        public string From { get; set; }
        public List<string> To { get; set; }
        public List<string> Cc { get; set; }

        public string Subject { get; set; }
        public string Body { get; set; }

        public User Owner { get; set; }
    }
}
