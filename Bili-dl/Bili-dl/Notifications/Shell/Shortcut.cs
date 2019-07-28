using System;
using System.IO;
using System.Windows;

namespace Shell
{
	/// <summary>
	/// Shortcut management
	/// </summary>
	public class Shortcut
	{
        public string ShortcutPath = null;
        public string TargetPath = null;
        public string Arguments = null;
        public string Comment = null;
        public string WorkingFolder = null;
        public WindowState WindowState = WindowState.Normal;
        public string IconPath = null;
        public string AppUserModelID = null;
        public Guid AppUserModelToastActivatorCLSID = Guid.Empty;

        public static bool ShortcutExist(Shortcut shortcut)
        {
            if (!File.Exists(shortcut.ShortcutPath))
                return false;

            using (ShellLink shellLink = new ShellLink(shortcut.ShortcutPath))
            {
                return (IsNullEmptyOrEquals(shellLink.TargetPath, shortcut.TargetPath, StringComparison.OrdinalIgnoreCase) &&
                    IsNullEmptyOrEquals(shellLink.Arguments, shortcut.Arguments, StringComparison.Ordinal) &&
                    IsNullEmptyOrEquals(shellLink.Description, shortcut.Comment, StringComparison.Ordinal) &&
                    IsNullEmptyOrEquals(shellLink.WorkingDirectory, shortcut.WorkingFolder, StringComparison.OrdinalIgnoreCase) &&
                    (shellLink.WindowStyle == ConvertToWindowStyle(shortcut.WindowState)) &&
                    IsNullEmptyOrEquals(shellLink.IconPath, shortcut.IconPath, StringComparison.OrdinalIgnoreCase) &&
                    IsNullEmptyOrEquals(shellLink.AppUserModelID, shortcut.AppUserModelID, StringComparison.Ordinal) &&
                    ((shortcut.AppUserModelToastActivatorCLSID == Guid.Empty) || (shellLink.AppUserModelToastActivatorCLSID == shortcut.AppUserModelToastActivatorCLSID)));
            }
        }

        public static void InstallShortcut(Shortcut shortcut)
        {
            if (string.IsNullOrWhiteSpace(shortcut.ShortcutPath))
                throw new ArgumentNullException(nameof(shortcut.ShortcutPath));

            using (ShellLink shellLink = new ShellLink
            {
                TargetPath = shortcut.TargetPath,
                Arguments = shortcut.Arguments,
                Description = shortcut.Comment,
                WorkingDirectory = shortcut.WorkingFolder,
                WindowStyle = ConvertToWindowStyle(shortcut.WindowState),
                IconPath = shortcut.IconPath,
                IconIndex = 0,
                AppUserModelID = shortcut.AppUserModelID
            })
            {
                if (shortcut.AppUserModelToastActivatorCLSID != Guid.Empty)
                    shellLink.AppUserModelToastActivatorCLSID = shortcut.AppUserModelToastActivatorCLSID;

                shellLink.Save(shortcut.ShortcutPath);
            }
        }

        public static void DeleteShortcut(Shortcut shortcut)
        {
            if (!ShortcutExist(shortcut))
                return;
            File.Delete(shortcut.ShortcutPath);
        }

        private static ShellLink.SW ConvertToWindowStyle(System.Windows.WindowState windowState)
        {
            switch (windowState)
            {
                case WindowState.Maximized:
                    return ShellLink.SW.SW_SHOWMAXIMIZED;
                case WindowState.Minimized:
                    return ShellLink.SW.SW_SHOWMINNOACTIVE;
                default:
                    return ShellLink.SW.SW_SHOWNORMAL;
            }
        }

        public static bool IsNullEmptyOrEquals(string a, string b, StringComparison stringComparison)
        {
            if (string.IsNullOrEmpty(a))
                return string.IsNullOrEmpty(b);

            return string.Equals(a, b, stringComparison);
        }
    }
}