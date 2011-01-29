using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EmailStatistics
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StartHelper());
        }

        public class StartHelper : ApplicationContext
        {
            public StartHelper()
            {
                //IController c = new Controller(new Model(), new View(), new GmailHelperFactory());
                //c.AppExit += new EventHandler(Controller_AppExit);
            }

            void Controller_AppExit(object sender, EventArgs e)
            {
                //Application.Exit();
            }
        }
    }
}
