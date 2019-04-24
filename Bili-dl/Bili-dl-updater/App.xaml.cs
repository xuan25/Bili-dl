using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Bili_dl_updater
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Application_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show("An unexpected and unrecoverable problem has occourred. \r\nThe software will now crash.\r\n\r\n" + string.Format("Captured an unhandled exception：\r\n{0}\r\n\r\nException Message：\r\n{1}\r\n\r\nException StackTrace：\r\n{2}", ex.GetType(), ex.Message, ex.StackTrace), "The software will now crash.", MessageBoxButton.OK, MessageBoxImage.Error);
            //Environment.Exit(0);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            MessageBox.Show("An unexpected problem has occourred. \r\nSome operation has been terminated.\r\n\r\n" + string.Format("Captured an unhandled exception：\r\n{0}\r\n\r\nException Message：\r\n{1}\r\n\r\nException StackTrace：\r\n{2}", ex.GetType(), ex.Message, ex.StackTrace), "Some operation has been terminated.", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Application.Current.Properties.Add("Args", e.Args);
        }
    }
}
