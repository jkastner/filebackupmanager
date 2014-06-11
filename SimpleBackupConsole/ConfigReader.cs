using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SimpleBackupConsole
{
    public class ConfigReader
    {
        public static bool ConfigError { get; set; }
        private const string SourceIndicator = "source";
        private const string DestinationIndicator = "destination";

        public static BackupPattern ReadBackup()
        {
            var bp = new BackupPattern("Main");
            DirectoryInfo baseDir = Directory.GetParent(Application.ExecutablePath);
            string backupPatternFile = baseDir + @"\Data\BackupPattern.txt";
            string configFile = baseDir + @"\Data\ConfigFile.txt";
            if (!File.Exists(backupPatternFile))
            {
                TextReporter.Report("Could not find config file - " + backupPatternFile, TextReporter.TextType.InitialError);
                ConfigError = true;
            }
            if (!File.Exists(configFile))
            {
                TextReporter.Report("Could not find config file - " + configFile, TextReporter.TextType.InitialError);
                ConfigError = true;
            }
            if (ConfigError)
            {
                return bp;
            }
            string[] allText = File.ReadAllLines(backupPatternFile);
            String curSource = "";
            foreach (var curPreTrim in allText)
            {
                var curLine = curPreTrim.Trim();
                if (String.IsNullOrWhiteSpace(curLine))
                {
                    continue;
                }
                if (curLine.Length > 0)
                {
                    //Comments marked by semicolon
                    if (curLine[0] == ';')
                    {
                        continue;
                    }
                }
                var curLineSplit = curLine.Split('\t');
                if (curLineSplit.Length < 2
                    ||
                    String.IsNullOrWhiteSpace(curLineSplit[0])
                    ||
                    String.IsNullOrWhiteSpace(curLineSplit[1]))
                {
                    TextReporter.Report("Error reading backup pattern file - could not parse line: " + curLine,
                        TextReporter.TextType.InitialError);
                    ConfigError = true;
                    break;
                }
                var curType = curLineSplit[0].Trim().ToLower();
                var curFolder = curLineSplit[1].Trim();
                if (curType.Contains(SourceIndicator))
                {
                    curSource = curFolder;
                }
                else if (curType.Contains(DestinationIndicator))
                {
                    if (String.IsNullOrWhiteSpace(curSource))
                    {
                        TextReporter.Report(
                            "Error reading backup pattern file - no source defined for: " + curFolder,
                            TextReporter.TextType.InitialError);
                        ConfigError = true;
                        return bp;
                    }
                    else
                    {
                        bp.AddBackup(new Source(curSource), new Destination(curFolder));
                    }
                }
                string[] configFileLines = File.ReadAllLines(configFile);
                ReadConfigfile(configFileLines);
                BackupRunnerViewModel.Instance.OnBackupPatternDescriptionChanged(new EventArgs());
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