using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EmailStatistics;
using System.Collections.ObjectModel;

namespace EmailStatisticsWPF
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window, IView
    {
        public ObservableCollection<ChartStatItem> _userChartStats { get; set; }
        public ObservableCollection<ChartStatItem> _hourChartStats { get; set; }
        public ObservableCollection<ChartStatItem> _dayChartStats { get; set; }

        IController _controller;
        IModel _model;

        ConnectionType _conType = ConnectionType.IMAP;
        string _host = "imap.gmail.com";
        int _port = 995;
        bool _useSSL = true;

        public Window1()
        {
            InitializeComponent();

            EmailObjectFactory factory = new EmailObjectFactory();
            Model m = new Model();
            Controller c = new Controller(m,this,factory);

            createChartDataSources(m);

            defaultState();
        }

        public void ShowView()
        { 
            // In WPF this method doesn't do anything
        }

        #region IView Members

        public void SetController(IController controller)
        {
            _controller = controller;
        }

        public void SetModel(IModel model)
        {
            _model = model;
            _model.ModelUpdated += new EventHandler(_model_ModelUpdated);
            _model.CountersUpdated += new EventHandler(_model_CountersUpdated);
        }

        void _model_CountersUpdated(object sender, EventArgs e)
        {
            countInbox.Content = _model.InboxCount;
            countSent.Content = _model.SentCount;
        }

        void _model_ModelUpdated(object sender, EventArgs e)
        {
            modelUpdated();
        }

        private void modelUpdated()
        {
            countSubject.Content = _model.SelectedCount;
            countProcessed.Content = string.Format("{0} % ({1})",_model.Process, _model.ProcessedCount);

            // TODO: This slows things down a lot

            // Update user stats
            foreach (var kvp in _model.UserStats)
            {
                ChartStatItem st = _userChartStats.Where(u => u.Name == kvp.Key).FirstOrDefault();

                if (st == null)
                    _userChartStats.Add(new ChartStatItem() { Name = kvp.Key, Value = kvp.Value });
                else
                    st.Value = kvp.Value;
            }

            // Update date stats (sunday modifications)
            int[] dayStats = _model.DateStats[StatType.Day];

            for (int i = 0; i < dayStats.Length -1 ; i++)
            {
                ChartStatItem st = _dayChartStats[i];
                st.Value = _model.DateStats[StatType.Day][i+1];
            }

            ChartStatItem sunDayItem = _dayChartStats[6];
            sunDayItem.Value = _model.DateStats[StatType.Day][0];

            // Update hour stats
            for (int i = 0; i < _model.DateStats[StatType.Hour1].Length; i++)
            {
                ChartStatItem st = _hourChartStats[i];
                st.Value = _model.DateStats[StatType.Hour1][i];
            }

        }

        public void SetConnectionOK()
        {
            messageLbl.Content = "Connected";
            connectedState();
        }

        public void SetConnectionError(string errorMessage)
        {
            messageLbl.Content = errorMessage;
            defaultState();
        }

        public void SetDisconnectOK()
        {
            countInbox.Content = "-";
            countSent.Content = "-";

            messageLbl.Content = "Disconnected";
            defaultState();
        }

        public void SetDisconnectError(string errorMessage)
        {
            messageLbl.Content = errorMessage;
            connectedState();
        }

        #endregion

        // TODO: Model only provides headers, fix
        private void createChartDataSources(IModel m)
        {
            _userChartStats = new ObservableCollection<ChartStatItem>();
            _hourChartStats = new ObservableCollection<ChartStatItem>();

            foreach (string st in m.GetStatsHeaders(StatType.Hour1))
            {
                ChartStatItem i = new ChartStatItem();
                i.Name = st;
                _hourChartStats.Add(i);
            }

            _dayChartStats = new ObservableCollection<ChartStatItem>();

            // I want sunday to be last so fill differetly
            string[] dayHeaders = m.GetStatsHeaders(StatType.Day);

            for (int i = 1; i < dayHeaders.Length; i++)
            {
                ChartStatItem it = new ChartStatItem();
                it.Name = dayHeaders[i];
                _dayChartStats.Add(it);
            }

            ChartStatItem sundayItem = new ChartStatItem();
            sundayItem.Name = dayHeaders[0];
            _dayChartStats.Add(sundayItem);

            userChart.DataContext = _userChartStats;
            userBarChart.DataContext = _userChartStats;
            dayChart.DataContext = _dayChartStats;
            hourChart.DataContext = _hourChartStats;
        }

        #region User info and checks

        private void usernameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkConnectButtonStates();
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            checkConnectButtonStates();
        }

        private void sentChkBox_Click(object sender, RoutedEventArgs e)
        {
            checkConnectButtonStates();
        }

        private void inboxChkBox_Click(object sender, RoutedEventArgs e)
        {
            checkConnectButtonStates();
        }

        private void checkConnectButtonStates()
        {
            if (usernameBox == null || passwordBox == null || inboxChkBox == null || sentChkBox == null)
                return;

            if (string.IsNullOrEmpty(usernameBox.Text) || string.IsNullOrEmpty(passwordBox.Password) || (!inboxChkBox.IsChecked.Value && !sentChkBox.IsChecked.Value))
            {
                connectBtn.IsEnabled = false;
            }
            else
            {
                connectBtn.IsEnabled = true;
            }
        }

        #endregion

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            messageLbl.Content = "Creating statistics";

            createChartDataSources(_model);

            gettingDataState();
          
            _controller.GetData(subjectBox.Text, inboxChkBox.IsChecked.Value, sentChkBox.IsChecked.Value);
        }
 
        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            messageLbl.Content = "Connecting";

            connectingState();

            _controller.SetConfig(_conType,_host,_port,_useSSL, usernameBox.Text, passwordBox.Password);
            _controller.EnableConnection();
        }

        private void disconnectBtn_Click(object sender, RoutedEventArgs e)
        {
            messageLbl.Content = "Disconnecting";

            disconnectingState();

            _controller.DisableConnection();
        }

        public void DataReady()
        {
            messageLbl.Content = "Statistics ready";

            connectedState();
        }

        #region Application view states

        void defaultState()
        {
            usernameBox.IsEnabled = true;
            passwordBox.IsEnabled = true;
            subjectBox.IsEnabled = true;

            inboxChkBox.IsEnabled = true;
            sentChkBox.IsEnabled = true;

            connectBtn.IsEnabled = false;
            disconnectBtn.IsEnabled = false;
            startBtn.IsEnabled = false;

            checkConnectButtonStates();
        }

        void connectingState()
        {
            usernameBox.IsEnabled = false;
            passwordBox.IsEnabled = false;
            subjectBox.IsEnabled = true;

            inboxChkBox.IsEnabled = true;
            sentChkBox.IsEnabled = true;

            connectBtn.IsEnabled = false;
            disconnectBtn.IsEnabled = false;
            startBtn.IsEnabled = false;
        }

        void connectedState()
        {
            usernameBox.IsEnabled = false;
            passwordBox.IsEnabled = false;
            subjectBox.IsEnabled = true;

            inboxChkBox.IsEnabled = true;
            sentChkBox.IsEnabled = true;

            connectBtn.IsEnabled = false;
            disconnectBtn.IsEnabled = true;
            startBtn.IsEnabled = true;
        }

        void disconnectingState()
        {
            usernameBox.IsEnabled = false;
            passwordBox.IsEnabled = false;
            subjectBox.IsEnabled = true;

            inboxChkBox.IsEnabled = true;
            sentChkBox.IsEnabled = true;

            connectBtn.IsEnabled = false;
            disconnectBtn.IsEnabled = false;
            startBtn.IsEnabled = false;
        }

        void gettingDataState()
        {
            usernameBox.IsEnabled = false;
            passwordBox.IsEnabled = false;
            subjectBox.IsEnabled = false;

            inboxChkBox.IsEnabled = false;
            sentChkBox.IsEnabled = false;

            connectBtn.IsEnabled = false;
            disconnectBtn.IsEnabled = false;
            startBtn.IsEnabled = false;
        }

        #endregion

    }
}
