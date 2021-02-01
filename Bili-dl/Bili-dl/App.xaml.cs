using Notifications;
using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Bili_dl
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.Resources.Add("Version", "v0.9.2-alpha");

            this.DispatcherUnhandledException += (object sender, DispatcherUnhandledExceptionEventArgs e) =>
            {
                Exception ex = e.Exception;
                MessageBox.Show("An unexpected problem has occourred. \r\nSome operation has been terminated.\r\n\r\n" + string.Format("Captured an unhandled exception：\r\n{0}\r\n\r\nException Message：\r\n{1}\r\n\r\nException StackTrace：\r\n{2}", ex.GetType(), ex.Message, ex.StackTrace), "Some operation has been terminated.", MessageBoxButton.OK, MessageBoxImage.Warning);
                e.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
            {
                Exception ex = (Exception)e.ExceptionObject;
                MessageBox.Show("An unexpected and unrecoverable problem has occourred. \r\nThe software will now crash.\r\n\r\n" + string.Format("Captured an unhandled exception：\r\n{0}\r\n\r\nException Message：\r\n{1}\r\n\r\nException StackTrace：\r\n{2}", ex.GetType(), ex.Message, ex.StackTrace), "The software will now crash.", MessageBoxButton.OK, MessageBoxImage.Error);
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length == 0)
            {
                new MainWindow().Show();
            }
            else
            {
                switch (e.Args[0].ToLower())
                {
                    case "-?":
                    case "-h":
                    case "-help":
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.AppendLine("-?\t Help");
                        stringBuilder.AppendLine("-h\t Help");
                        stringBuilder.AppendLine("-help\t Help");
                        stringBuilder.AppendLine("-uninstall\t Uninstall all registered service suppoets of Bili-dl " +
                                                 " \t\t\t included the Shortcut in Start menu");
                        MessageBox.Show(stringBuilder.ToString());
                        Environment.Exit(0);
                        break;
                    case "-uninstall":
                        NotificationManager.Uninstall();
                        MessageBox.Show("Uninstalled successfully");
                        Environment.Exit(0);
                        break;
                    case "-update":
                        UpdateUtil.RunUpdate();
                        Environment.Exit(0);
                        break;
                    default:
                        if (e.Args[0] == NotificationManager.ToastActivatedLaunchArg)
                        {
                            ToastHandler.CmdMode = true;
                            NotificationManager.Activated += ToastHandler.HandleToast;
                            NotificationManager.Install();
                        }
                        else
                        {
                            MessageBox.Show("Invalid arguments");
                            Environment.Exit(0);
                        }
                        break;
                }
            }
        }
    }
}
