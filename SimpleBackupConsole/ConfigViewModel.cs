using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleBackupConsole
{
    public class ConfigViewModel
    {
        public static bool ConfigError { get; set; }
        private static ConfigViewModel _instance;

        private ConfigViewModel()
        {
            
        }
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


        /*Config file format
        * RunAutomatically
        * ShutdownOnCompletion
        * StaggerBackup
        * TargetDay (irrelevant if the top is false)
        */
        private static void ReadConfigfile(string[] configFileLines)
        {
            BackupRunnerViewModel.Instance.RunAutomatically = bool.Parse(configFileLines[0]);
            BackupRunnerViewModel.Instance.ShutdownOnCompletion = bool.Parse(configFileLines[1]);
            BackupRunnerViewModel.Instance.StaggerBackup = bool.Parse(configFileLines[2]);
            if (BackupRunnerViewModel.Instance.RunAutomatically)
            {
                var targetDaysInfo = configFileLines[3].Trim().Split(null);
                bool shouldAddToday = false;
                BackupRunnerViewModel.Instance.TargetDays.Clear();
                foreach (var curDay in targetDaysInfo)
                {
                    DayOfWeek targetDay = DateTime.Now.DayOfWeek;
                    bool parseSuccessfuly = Enum.TryParse(curDay, true, out targetDay);
                    if (parseSuccessfuly)
                    {
                        BackupRunnerViewModel.Instance.TargetDays.Add(targetDay);
                    }
                    else
                    {
                        shouldAddToday = true;
                    }
                }

                if (!targetDaysInfo.Any() || shouldAddToday)
                {
                    BackupRunnerViewModel.Instance.TargetDays.Add(DateTime.Now.DayOfWeek);
                }
            }
            if (BackupRunnerViewModel.Instance.StaggerBackup && DateTime.Now.Day%2 == 0)
            {
                BackupRunnerViewModel.Instance.StaggerBackup = true;
            }
            else if (BackupRunnerViewModel.Instance.StaggerBackup)
            {
                BackupRunnerViewModel.Instance.StaggerBackup = false;
            }
        }
    }
}
