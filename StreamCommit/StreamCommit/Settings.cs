using PropertyChanged;
using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;

namespace StreamCommit
{
    [AddINotifyPropertyChangedInterface]
    public class Settings : INotifyPropertyChanged
    {
        public static Settings Instance { get; private set; }

        private static string defaultAppSettings = $"<?xml version=\"1.0\" encoding=\"utf-8\" ?>{Environment.NewLine}<configuration>{Environment.NewLine}\t<startup> {Environment.NewLine}\t\t<supportedRuntime version=\"v4.0\" sku=\".NETFramework,Version=v4.5.2\" />{Environment.NewLine}\t</startup>{Environment.NewLine}</configuration>";

        private Configuration _configuration;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FolderToWatch
        {
            get
            {
                return GetSetting(nameof(FolderToWatch));
            }
            set
            {
                SetSetting(nameof(FolderToWatch), value);
            }
        }

        public bool HasCredentials
        {
            get
            {
                return !(string.IsNullOrWhiteSpace(GitUsername) || string.IsNullOrWhiteSpace(GitPassword) || string.IsNullOrWhiteSpace(GitEmail));
            }
        }

        public string GitUsername
        {
            get
            {
                return GetSetting(nameof(GitUsername));
            }
            set
            {
                SetSetting(nameof(GitUsername), value);
            }
        }

        public string GitPassword
        {
            get
            {
                return GetSetting(nameof(GitPassword));
            }
            set
            {
                SetSetting(nameof(GitPassword), value);
            }
        }

        public string GitEmail
        {
            get
            {
                return GetSetting(nameof(GitEmail));
            }
            set
            {
                SetSetting(nameof(GitEmail), value);
            }
        }

        public int CommitInterval
        {
            get
            {
                return GetInt(nameof(CommitInterval), 10);
            }
            set
            {
                SetSetting(nameof(CommitInterval), value.ToString());
            }
        }

        public Settings()
        {
            if (!File.Exists(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile))
            {
                File.WriteAllText(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, defaultAppSettings);
            }
            _configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            Instance = this;
        }

        public void Save()
        {
            _configuration.Save(ConfigurationSaveMode.Modified);
        }

        private string GetSetting(string setting)
        {
            if (_configuration.AppSettings.Settings[setting] == null)
                SetSetting(setting, "");

            return _configuration.AppSettings.Settings[setting].Value;
        }

        private int GetInt(string setting, int defaultSetting)
        {
            if (_configuration.AppSettings.Settings[setting] == null)
                SetSetting(setting, defaultSetting.ToString());
            int tmp;
            if (int.TryParse(_configuration.AppSettings.Settings[setting].Value, out tmp))
            {
                return tmp;
            }
            else
            {
                SetSetting(setting, defaultSetting.ToString());
                return defaultSetting;
            }
        }

        private void SetSetting(string setting, string value)
        {
            if (!_configuration.AppSettings.Settings.AllKeys.Contains(setting))
            {
                _configuration.AppSettings.Settings.Add(setting, value);
            }
            else
            {
                _configuration.AppSettings.Settings[setting].Value = value;
            }
        }
    }
}