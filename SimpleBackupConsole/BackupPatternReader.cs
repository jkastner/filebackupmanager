using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SimpleBackupConsole
{
    public class BackupPatternReader
    {

        private const string SourceIndicator = "source";
        private const string DestinationIndicator = "destination";
        public static bool ReadError { get; set; }

        public static BackupPattern ReadBackup()
        {
            var bp = new BackupPattern("Main");
            DirectoryInfo baseDir = Directory.GetParent(Application.ExecutablePath);
            string backupPatternFile = baseDir + @"\Data\BackupPattern.txt";
            
            if (!File.Exists(backupPatternFile))
            {
                TextReporter.Report("Could not find config file - " + backupPatternFile, TextReporter.TextType.InitialError);
                ReadError = true;
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
                    ConfigViewModel.ConfigError = true;
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
                        ConfigViewModel.ConfigError = true;
                        return bp;
                    }
                    else
                    {
                        bp.AddBackup(new Source(curSource), new Destination(curFolder));
                    }
                }
                BackupRunnerViewModel.Instance.OnBackupPatternDescriptionChanged(new EventArgs());
            }
            return bp;
        }


    }
}