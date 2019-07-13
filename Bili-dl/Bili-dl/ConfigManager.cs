using Bili_dl;
using BiliDownload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ConfigManager
{
    /// <summary>
    /// Class <c>ConfigManager</c> used to storing/loading configuration.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    class ConfigManager
    {
        [Serializable]
        public class Config
        {
            public bool StatementConfirmed;
            public CookieCollection CookieCollection;
            public List<DownloadInfo> DownloadInfos;
            public SettingPanel.Settings Settings;
            public List<string> SearchHistory;

            public Config()
            {
                StatementConfirmed = false;
                DownloadInfos = new List<DownloadInfo>();
                Settings = new SettingPanel.Settings();
                SearchHistory = new List<string>();
            }
        }

        // Current config instance.
        private static Config config;
        // The filepath of the config file.
        private static string configPath;

        /// <summary>
        /// Load/Create a config instance
        /// </summary>
        public static void Init()
        {
            configPath = AppDomain.CurrentDomain.BaseDirectory + "Config.dat";
            if (File.Exists(configPath))
            {
                config = Deserialize();

                if (config.DownloadInfos == null)
                    config.DownloadInfos = new List<DownloadInfo>();
                if (config.Settings == null)
                    config.Settings = new SettingPanel.Settings();
                if (config.SearchHistory == null)
                    config.SearchHistory = new List<string>();
            }
            else
                config = new Config();
        }

        public static void ConfirmStatement()
        {
            config.StatementConfirmed = true;
            Serialize();
        }

        public static bool GetStatementConfirmed()
        {
            return config.StatementConfirmed;
        }

        public static void SetCookieCollection(CookieCollection cookieCollection)
        {
            config.CookieCollection = cookieCollection;
            Serialize();
        }

        public static CookieCollection GetCookieCollection()
        {
            return config.CookieCollection;
        }

        public static void AppendDownloadInfo(DownloadInfo downloadInfo)
        {
            if (!config.DownloadInfos.Contains(downloadInfo))
            {
                config.DownloadInfos.Add(downloadInfo);
                Serialize();
            }
        }

        public static void RemoveDownloadInfo(DownloadInfo downloadInfo)
        {
            if (config.DownloadInfos.Contains(downloadInfo))
            {
                config.DownloadInfos.Remove(downloadInfo);
                Serialize();
            }
        }

        public static List<DownloadInfo> GetDownloadInfos()
        {
            return config.DownloadInfos;
        }

        public static void SetSettings(SettingPanel.Settings settings)
        {
            config.Settings = settings;
            Serialize();
        }

        public static SettingPanel.Settings GetSettings()
        {
            return config.Settings;
        }

        public static void SetSearchHistory(List<string> searchHistory)
        {
            config.SearchHistory = searchHistory;
            Serialize();
        }

        public static List<string> GetSearchHistory()
        {
            return config.SearchHistory;
        }

        private static void Serialize()
        {
            FileStream fileStream = new FileStream(configPath, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fileStream, config);
            }
            catch (SerializationException)
            {
                //Console.WriteLine("Failed to serialize. Reason: " + e.Message);
            }
            finally
            {
                fileStream.Close();
            }
        }

        private static Config Deserialize()
        {
            Config config = null;
            FileStream fileStream = new FileStream(configPath, FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                config = (Config)formatter.Deserialize(fileStream);
            }
            catch (SerializationException)
            {
                //Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                config = new Config();
            }
            finally
            {
                fileStream.Close();
            }
            return config;
        }
    }
}
