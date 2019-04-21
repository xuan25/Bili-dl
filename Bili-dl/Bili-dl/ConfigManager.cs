using BiliDownload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ConfigManager
{
    class ConfigManager
    {
        [Serializable]
        public class Config
        {
            public bool StatementConfirmed;
            public CookieCollection CookieCollection;
            public List<DownloadInfo> DownloadInfos;

            public Config()
            {
                StatementConfirmed = false;
                DownloadInfos = new List<DownloadInfo>();
            }
        }

        private static Config config;

        private static string configPath;

        public static void Init()
        {
            configPath = AppDomain.CurrentDomain.BaseDirectory + "Config.dat";
            if (File.Exists(configPath))
                config = Deserialize();
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

        private static void Serialize()
        {
            FileStream fileStream = new FileStream(configPath, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fileStream, config);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
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
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
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
