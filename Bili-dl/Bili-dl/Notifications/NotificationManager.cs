using Microsoft.Win32;
using Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Notifications
{
    /// <summary>
    /// Static class NotificationManager
    /// Author: Xuan525
    /// Date: 28/07/2019
    /// </summary>
    public static class NotificationManager
    {
        #region NotificationActivator

        [ClassInterface(ClassInterfaceType.None)]
        [ComSourceInterfaces(typeof(INotificationActivationCallback))]
        [Guid("02CD48A5-355C-40B7-B903-2ED4AA637782"), ComVisible(true)]
        public class NotificationActivator : INotificationActivationCallback
        {
            public void Activate(string appUserModelId, string invokedArgs, NotificationUserInputData[] data, uint dataCount)
            {
                OnActivated(invokedArgs, new NotificationUserInput(data), appUserModelId);
            }
        }

        #endregion

        #region Other classes

        [StructLayout(LayoutKind.Sequential), Serializable]
        public struct NotificationUserInputData
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Key;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Value;
        }

        [ComImport,
        Guid("53E31837-6600-4A81-9395-75CFFE746F94"), ComVisible(true),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface INotificationActivationCallback
        {
            void Activate(
                [In, MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
                [In, MarshalAs(UnmanagedType.LPWStr)] string invokedArgs,
                [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] NotificationUserInputData[] data,
                [In, MarshalAs(UnmanagedType.U4)] uint dataCount);
        }

        public class NotificationUserInput : IReadOnlyDictionary<string, string>
        {
            private NotificationUserInputData[] data;

            internal NotificationUserInput(NotificationUserInputData[] data)
            {
                this.data = data;
            }

            public string this[string key] => data.First(i => i.Key == key).Value;

            public IEnumerable<string> Keys => data.Select(i => i.Key);

            public IEnumerable<string> Values => data.Select(i => i.Value);

            public int Count => data.Length;

            public bool ContainsKey(string key)
            {
                return data.Any(i => i.Key == key);
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                return data.Select(i => new KeyValuePair<string, string>(i.Key, i.Value)).GetEnumerator();
            }

            public bool TryGetValue(string key, out string value)
            {
                foreach (var item in data)
                {
                    if (item.Key == key)
                    {
                        value = item.Value;
                        return true;
                    }
                }

                value = null;
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class NotificationHistory
        {
            private string aumid;
            private ToastNotificationHistory history;

            internal NotificationHistory(string aumid)
            {
                this.aumid = aumid;
                history = ToastNotificationManager.History;
            }

            public void Clear()
            {
                history.Clear(aumid);
            }

            public IReadOnlyList<ToastNotification> GetHistory()
            {
                return history.GetHistory(aumid);
            }

            public void Remove(string tag)
            {
                history.Remove(tag, string.Empty, aumid);
            }

            public void Remove(string tag, string group)
            {
                history.Remove(tag, group, aumid);
            }

            public void RemoveGroup(string group)
            {
                history.RemoveGroup(group, aumid);
            }
        }

        #endregion

        #region Consts

        public const string ToastActivatedLaunchArg = "-ToastActivated";

        #endregion

        #region Private properties

        private static string aumid;
        private static int? cookie;

        #endregion

        #region Private methods

        private static void RegisterComServer()
        {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            string regString = string.Format("SOFTWARE\\Classes\\CLSID\\{{{0}}}\\LocalServer32", typeof(NotificationActivator).GUID);
            RegistryKey key = Registry.CurrentUser.CreateSubKey(regString);
            key.SetValue(null, '"' + exePath + '"' + " " + ToastActivatedLaunchArg);
        }

        private static void UnregisterComServer()
        {
            string regString = string.Format("SOFTWARE\\Classes\\CLSID\\{{{0}}}", typeof(NotificationActivator).GUID);
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(regString))
            {
                if (key == null)
                    return;
            }
            Registry.CurrentUser.DeleteSubKeyTree(regString);

            aumid = null;
        }

        private static void RegisterActivator()
        {
            RegistrationServices regService = new RegistrationServices();
            regService.RegisterTypeForComClients(typeof(NotificationActivator), RegistrationClassContext.LocalServer, RegistrationConnectionType.MultipleUse);
        }

        private static void UnregisterActivator()
        {
            if (cookie == null)
                return;
            RegistrationServices regService = new RegistrationServices();
            regService.UnregisterTypeForComClients((int)cookie);
            cookie = null;
        }

        private static Shortcut CreateShortcut()
        {
            Shortcut shortcut = new Shortcut()
            {
                ShortcutPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                    "Programs",
                    string.Format(
                        "{0}.lnk",
                        Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product)),
                TargetPath = Assembly.GetExecutingAssembly().Location,
                AppUserModelID = aumid,
                AppUserModelToastActivatorCLSID = typeof(NotificationActivator).GUID
            };
            return shortcut;
        }

        private static void OnActivated(string arguments, NotificationUserInput userInput, string appUserModelId)
        {
            Activated?.Invoke(arguments, userInput, appUserModelId);
        }

        #endregion

        #region Events

        public delegate void ActivatedHandler(string arguments, NotificationUserInput userInput, string appUserModelId);
        public static event ActivatedHandler Activated;

        #endregion

        #region Static constructor

        static NotificationManager()
        {
            AssemblyProductAttribute productAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>();
            AssemblyCompanyAttribute companyAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCompanyAttribute>();
            if (string.IsNullOrEmpty(companyAttribute.Company))
                aumid = productAttribute.Product;
            else
                aumid = string.Format("{0}.{1}", companyAttribute.Company, productAttribute.Product);
        }

        #endregion

        #region Public methods

        public static void Install()
        {
            RegisterComServer();
            RegisterActivator();

            Shortcut shortcut = CreateShortcut();
            if (!Shortcut.ShortcutExist(shortcut))
                Shortcut.InstallShortcut(shortcut);
        }

        public static void Close()
        {
            UnregisterActivator();
        }

        public static void Uninstall()
        {
            UnregisterComServer();
            UnregisterActivator();

            Shortcut shortcut = CreateShortcut();
            Shortcut.DeleteShortcut(shortcut);
        }

        public static ToastNotifier Show(string xml)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            return Show(xmlDocument);
        }

        public static ToastNotifier Show(XmlDocument xmlDocument)
        {
            ToastNotification toastNotification = new ToastNotification(xmlDocument);
            return Show(toastNotification);
        }

        public static ToastNotifier Show(ToastNotification toastNotification)
        {
            ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier(aumid);
            toastNotifier.Show(toastNotification);

            return toastNotifier;
        }

        #endregion

        #region Public properties

        public static NotificationHistory History
        {
            get
            {
                return new NotificationHistory(aumid);
            }
        }

        #endregion
    }
}