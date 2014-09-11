using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace SimpleBackupConsole
{
    public class ConfigViewModel : INotifyPropertyChanged
    {
        private static ConfigViewModel _instance;

        private bool _runAutomatically = true;
        private bool _shutdownOnCompletion;
        private bool _staggerBackup = true;
        private HashSet<DayOfWeek> _targetDays = new HashSet<DayOfWeek>();

        private ConfigViewModel()
        {
            _targetDays.Add(DateTime.Now.DayOfWeek);
        }

        public static bool ConfigError { get; set; }

        public static ConfigViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConfigViewModel();
                }
                return _instance;
            }
        }

        public HashSet<DayOfWeek> TargetDays
        {
            get { return _targetDays; }
            set
            {
                _targetDays = value;
                OnPropertyChanged();
            }
        }


        public bool ShutdownOnCompletion
        {
            get { return _shutdownOnCompletion; }
            set
            {
                _shutdownOnCompletion = value;
                OnPropertyChanged();
            }
        }

        public bool StaggerBackup
        {
            get { return _staggerBackup; }
            set
            {
                _staggerBackup = value;
                OnPropertyChanged();
                BackupRunnerViewModel.Instance.OnBackupPatternDescriptionChanged(new EventArgs());
            }
        }

        public bool RunAutomatically
        {
            get { return _runAutomatically; }
            set
            {
                _runAutomatically = value;
                OnPropertyChanged();
            }
        }

        /*Config file format
        * RunAutomatically
        * ShutdownOnCompletion
        * StaggerBackup
        * TargetDay (irrelevant if the top is false)
        */

        public event PropertyChangedEventHandler PropertyChanged;

        public void ReadConfigData()
        {
            DirectoryInfo baseDir = Directory.GetParent(Application.ExecutablePath);
            string configFile = baseDir + @"\Data\ConfigFile.txt";
            if (!File.Exists(configFile))
            {
                TextReporter.Report("Could not find config file - " + configFile, TextReporter.TextType.InitialError);
                ConfigError = true;
            }
            string[] configFileLines = File.ReadAllLines(configFile);
            ReadConfigfile(configFileLines);
        }

        private void ReadConfigfile(string[] configFileLines)
        {
            RunAutomatically = bool.Parse(configFileLines[0]);
            ShutdownOnCompletion = bool.Parse(configFileLines[1]);
            StaggerBackup = bool.Parse(configFileLines[2]);
            if (RunAutomatically)
            {
                string[] targetDaysInfo = configFileLines[3].Trim().Split(null);
                bool shouldAddToday = false;
                TargetDays.Clear();
                foreach (string curDay in targetDaysInfo)
                {
                    DayOfWeek targetDay = DateTime.Now.DayOfWeek;
                    bool parseSuccessfuly = Enum.TryParse(curDay, true, out targetDay);
                    if (parseSuccessfuly)
                    {
                        TargetDays.Add(targetDay);
                    }
                    else
                    {
                        shouldAddToday = true;
                    }
                }

                if (!targetDaysInfo.Any() || shouldAddToday)
                {
                    TargetDays.Add(DateTime.Now.DayOfWeek);
                }
            }
            if (StaggerBackup && DateTime.Now.Day%2 == 0)
            {
                StaggerBackup = true;
            }
            else if (StaggerBackup)
            {
                StaggerBackup = false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}