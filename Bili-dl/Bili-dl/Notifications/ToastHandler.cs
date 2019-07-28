using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Notifications
{
    /// <summary>
    /// Static class ToastHandler
    /// Author: Xuan525
    /// Date: 28/07/2019
    /// </summary>
    public static class ToastHandler
    {
        public static bool CmdMode = false;

        public static void HandleToast(string arguments, NotificationManager.NotificationUserInput userInput, string appUserModelId)
        {
            foreach (string arg in arguments.Split('&'))
            {
                string[] argArr = arg.Split('=');
                switch (argArr[0])
                {
                    case "open":
                        Process.Start(argArr[1]);
                        break;
                    case "openfolder":
                        Process.Start("explorer", string.Format("/select,\"{0}\"", argArr[1]));
                        break;
                    case "move":
                        Thread thread = new Thread(delegate ()
                        {
                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.Title = "移动至";
                            saveFileDialog.DefaultExt = Path.GetExtension(argArr[1]);
                            saveFileDialog.Filter = string.Format("视频文件|*{0}", Path.GetExtension(argArr[1]));
                            saveFileDialog.FileName = Path.GetFileName(argArr[1]);
                            saveFileDialog.AddExtension = true;
                            switch (saveFileDialog.ShowDialog())
                            {
                                case DialogResult.OK:
                                case DialogResult.Yes:
                                    File.Copy(argArr[1], saveFileDialog.FileName, true);
                                    File.Delete(argArr[1]);
                                    break;
                            }
                        });
                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();
                        thread.Join();
                        break;
                }
            }

            if (CmdMode)
            {
                NotificationManager.Close();
                Environment.Exit(0);
            }
        }
    }
}
