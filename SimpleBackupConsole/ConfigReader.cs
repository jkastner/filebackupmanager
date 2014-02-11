using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleBackupConsole
{
    public class ConfigReader
    {
        private static bool _configError;

        public static bool ConfigError
        {
            get { return _configError; }
            set { _configError = value; }
        }

        public static BackupPattern ReadBackup()
        {
            BackupPattern bp = new BackupPattern("Main");
            var baseDir = Directory.GetParent(Application.ExecutablePath);
            string backupFile = baseDir+@"\Data\BackupTargets.txt";
            string directoriesToHoldBackupsFile = baseDir + @"\Data\DirectoriesToHoldBackups.txt";
            string configFile = baseDir + @"\Data\ConfigFile.txt";
            List<String> directoriesToBackup = new List<string>();
            List<String> directoriesToHoldBackups = new List<string>();
            if (!File.Exists(backupFile))
            {
                TextReporter.Report("Could not find config file - " + backupFile, TextReporter.TextType.InitialError);
                ConfigError = true;
            }
            if (!File.Exists(directoriesToHoldBackupsFile))
            {
                TextReporter.Report("Could not find config file - " + directoriesToHoldBackupsFile, TextReporter.TextType.InitialError);
                ConfigError = true;
            }
            if (!File.Exists(configFile))
            {
                TextReporter.Report("Could not find config file - " + configFile, TextReporter.TextType.InitialError);
                ConfigError = true;
            }
            else
            {
                var allTargets = File.ReadAllLines(backupFile);
                foreach (var cur in allTargets)
                {
                    String curFix = cur.Trim();
                    if (!String.IsNullOrEmpty(curFix))
                    {
                        directoriesToBackup.Add(curFix);
                    }
                }
                var lines = File.ReadAllLines(directoriesToHoldBackupsFile);
                foreach (var cur in lines)
                {
                    if (!String.IsNullOrWhiteSpace(cur))
                    {
                        directoriesToHoldBackups.Add(cur);
                    }
                }
                var configFileLines = File.ReadAllLines(configFile);
                ReadConfigfile(configFileLines);
                BackupRunnerViewModel.Instance.OnBackupPatternDescriptionChanged(new EventArgs());

            }
            foreach (var cursource in directoriesToBackup)
            {
                foreach (var curdest in directoriesToHoldBackups)
                {
                    bp.AddBackup(new Source(cursource), new Destination(curdest));
                }
            }
            return bp;
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
                var targetDay = DateTime.Now.DayOfWeek;
                bool parseSuccessfuly = DayOfWeek.TryParse(configFileLines[3], true, out targetDay);
                if (!parseSuccessfuly)
                {
                    BackupRunnerViewModel.Instance.TargetDay = DateTime.Now.DayOfWeek;
                }
                else
                {
                    BackupRunnerViewModel.Instance.TargetDay = targetDay;
                }
            }
            if (BackupRunnerViewModel.Instance.StaggerBackup && DateTime.Now.Day % 2 == 0)
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
