using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SimpleBackupConsole
{
    public class ConfigReader
    {
        public static bool ConfigError { get; set; }

        public static BackupPattern ReadBackup()
        {
            var bp = new BackupPattern("Main");
            DirectoryInfo baseDir = Directory.GetParent(Application.ExecutablePath);
            string backupFile = baseDir + @"\Data\BackupTargets.txt";
            string directoriesToHoldBackupsFile = baseDir + @"\Data\DirectoriesToHoldBackups.txt";
            string configFile = baseDir + @"\Data\ConfigFile.txt";
            var directoriesToBackup = new List<string>();
            var directoriesToHoldBackups = new List<string>();
            if (!File.Exists(backupFile))
            {
                TextReporter.Report("Could not find config file - " + backupFile, TextReporter.TextType.InitialError);
                ConfigError = true;
            }
            if (!File.Exists(directoriesToHoldBackupsFile))
            {
                TextReporter.Report("Could not find config file - " + directoriesToHoldBackupsFile,
                    TextReporter.TextType.InitialError);
                ConfigError = true;
            }
            if (!File.Exists(configFile))
            {
                TextReporter.Report("Could not find config file - " + configFile, TextReporter.TextType.InitialError);
                ConfigError = true;
            }
            else
            {
                string[] allTargets = File.ReadAllLines(backupFile);
                foreach (string cur in allTargets)
                {
                    String curFix = cur.Trim();
                    if (!String.IsNullOrEmpty(curFix))
                    {
                        directoriesToBackup.Add(curFix);
                    }
                }
                string[] lines = File.ReadAllLines(directoriesToHoldBackupsFile);
                foreach (string cur in lines)
                {
                    if (!String.IsNullOrWhiteSpace(cur))
                    {
                        directoriesToHoldBackups.Add(cur);
                    }
                }
                string[] configFileLines = File.ReadAllLines(configFile);
                ReadConfigfile(configFileLines);
                BackupRunnerViewModel.Instance.OnBackupPatternDescriptionChanged(new EventArgs());
            }
            foreach (string cursource in directoriesToBackup)
            {
                foreach (string curdest in directoriesToHoldBackups)
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
                DayOfWeek targetDay = DateTime.Now.DayOfWeek;
                bool parseSuccessfuly = Enum.TryParse(configFileLines[3], true, out targetDay);
                if (!parseSuccessfuly)
                {
                    BackupRunnerViewModel.Instance.TargetDay = DateTime.Now.DayOfWeek;
                }
                else
                {
                    BackupRunnerViewModel.Instance.TargetDay = targetDay;
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