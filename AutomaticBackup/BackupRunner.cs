using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AutomaticBackup
{
    public class BackupRunner
    {
        private static bool _debugOnly = false;
        private static BackupRunner _instance;
        private long _currentMax;
        private long _currentProgress;
        private long _overallMax;
        private long _overallProgress;

        private int _tabIndex;

        private BackupRunner()
        {
        }

        public static BackupRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BackupRunner();
                }
                return _instance;
            }
        }

        protected string Indentation
        {
            get
            {
                var sb = new StringBuilder();
                for (int curIndex = 0; curIndex < _tabIndex; curIndex++)
                {
                    sb.Append("\t");
                }
                return sb.ToString();
            }
        }

        public void StartBackup(BackupPattern currentBackup)
        {
            if (!ConfigViewModel.Instance.TargetDays.Contains(DateTime.Now.Date.DayOfWeek)
                &&BackupRunnerViewModel.Instance.RunAutomatically)
            {
                BackupRunnerViewModel.Instance.CloseUIOnCompleted = true;
                string dayStrings = string.Join(",", ConfigViewModel.Instance.TargetDays.ToArray());
                ReportWithIndentation("Targets day " + dayStrings + " did not match current day " +
                    DateTime.Now.DayOfWeek, TextReporter.TextType.Output);
                BackupRunnerViewModel.Instance.ShouldWriteLog = false;
                BackupRunnerViewModel.Instance.ShouldClose = true;
                BackupRunnerViewModel.Instance.OnBackupCompleted(new EventArgs());
                return;
            }
            if (!currentBackup.Pattern.Any())
            {
                BackupRunnerViewModel.Instance.OnBackupCompleted(new EventArgs());
                return;
            }
            if (!AllTargetFilesExist(currentBackup))
            {
                BackupRunnerViewModel.Instance.OnBackupCompleted(new EventArgs());
                return;
            }
            bool shouldStagger = false;
            if (ConfigViewModel.Instance.StaggerBackup && DateTime.Now.Day % 2 == 0)
            {
                shouldStagger = true;
            }
            else if (ConfigViewModel.Instance.StaggerBackup)
            {
                shouldStagger = false;
            }
            DateTime startTime = DateTime.Now;

            ReportWithIndentation("Backup started at " + DateTime.Now, TextReporter.TextType.Output);
            if (ConfigViewModel.Instance.CalculateCopyTime)
            {
                ReportWithIndentation("Estimating copy size...", TextReporter.TextType.Output);
            }
            else
            {
                ReportWithIndentation("Skipping copy size estimation...", TextReporter.TextType.Output);
            }
            var directorySizes = new List<Tuple<Tuple<Source, Destination>, long>>();
            if (ConfigViewModel.Instance.CalculateCopyTime)
            {
                directorySizes = GetAllDirectorySizes(currentBackup);
            }
            _overallMax = directorySizes.Sum(x => x.Item2);
            foreach (var curPair in currentBackup.Pattern)
            {
                Source curSource = curPair.Key;
                ReportWithIndentation("Starting backup of: " + curSource.BackupSource, TextReporter.TextType.Output);
                foreach (Destination curDestination in curPair.Value)
                {
                    try
                    {
                        Thread.Sleep(2000);
                        if (!Directory.Exists(curDestination.BackupDestination))
                        {
                            ReportWithIndentation( "Creating " + curDestination.BackupDestination,
                                TextReporter.TextType.Output);
                            Directory.CreateDirectory(curDestination.BackupDestination);
                        }
                        string currentSource = curSource.BackupSource;
                        try
                        {
                            if (directorySizes.Any())
                            {
                                _currentMax =
                                    directorySizes.First(
                                        x => x.Item1.Item1.Equals(curSource) && x.Item1.Item2.Equals(curDestination))
                                        .Item2;
                            }
                            _currentProgress = 0;
                            string TargetDir = currentBackup.UniqueFinalPath(curSource, curDestination, shouldStagger);
                            _tabIndex++;
                            ReportWithIndentation( "Copying " + currentSource + " to " + TargetDir,
                                TextReporter.TextType.Output);
                            _tabIndex++;
                            ReportWithIndentation( "Copying files...", TextReporter.TextType.Output);
                            HandleBackup(currentSource, TargetDir);
                            ReportWithIndentation( "Cleaning old files from " + TargetDir,
                                TextReporter.TextType.Output);
                            HandleCleanup(currentSource, TargetDir);
                            _tabIndex--;
                            ReportWithIndentation( "Completed copy of " + currentSource + " to " + TargetDir,
                                TextReporter.TextType.Output);
                        }
                        catch (Exception e)
                        {
                            _tabIndex++;
                            ReportWithIndentation(
                                 "ERROR: Could not copy " + currentSource + " to " +
                                curDestination.BackupDestination + "\n" +
                                Indentation + "Exception: " + e.Message, TextReporter.TextType.BackupError);
                            _tabIndex--;
                        }
                    }
                    catch (Exception e)
                    {
                        ReportWithIndentation("ERROR: " + e.Message, TextReporter.TextType.BackupError);
                    }
                    _tabIndex--;
                }
            }
            DateTime endTime = DateTime.Now;
            double backupDuration = Math.Round((endTime - startTime).TotalMinutes, 1);
            ReportWithIndentation("Backup ended at " + DateTime.Now, TextReporter.TextType.Output);
            ReportWithIndentation("Duration:  " + backupDuration + " minutes", TextReporter.TextType.Output);
            BackupRunnerViewModel.Instance.OnBackupCompleted(new EventArgs());
        }


        private List<Tuple<Tuple<Source, Destination>, long>> GetAllDirectorySizes(BackupPattern currentBackup)
        {
            var info = new List<Tuple<Tuple<Source, Destination>, long>>();
            foreach (var curPair in currentBackup.Pattern)
            {
                long curSize = GetDirectorySize(new DirectoryInfo(curPair.Key.BackupSource));
                foreach (Destination curDest in curPair.Value)
                {
                    info.Add(
                        new Tuple<Tuple<Source, Destination>, long>(
                            new Tuple<Source, Destination>(curPair.Key, curDest), curSize));
                }
            }
            return info;
        }

        /// <summary>
        ///     Directory.delete stops if even a single targetFile is write-protected.
        ///     Use this helper method instead
        /// </summary>
        /// <param name="target_dir"></param>
        public void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                try
                {
                    DeleteFile(file);
                }
                catch (Exception e)
                {
                    TextReporter.ReportDeleteError("Could not delete targetFile: " + file, file, e);
                }
            }
            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }
            try
            {
                DeleteDirectoryCall(target_dir);
            }
            catch (Exception e)
            {
                TextReporter.ReportDeleteError("Could not delete directory: " + target_dir, target_dir, e);
            }
        }

        private void DeleteDirectoryCall(string target)
        {
            if (_debugOnly)
            {
                ReportWithIndentation("Would delete " + target, TextReporter.TextType.Output);
            }
            else
            {
                Directory.Delete(target, false);
            }
        }

        private void DeleteFile(string file)
        {
            if (_debugOnly)
            {
                ReportWithIndentation("Would delete " + file, TextReporter.TextType.Output);
            }
            else
            {
                File.Delete(file);
            }
        }

        public static long GetDirectorySize(DirectoryInfo dir)
        {
            return dir.GetFiles().Sum(fi => fi.Length) +
                   dir.GetDirectories().Sum(di => GetDirectorySize(di));
        }

        private void HandleBackup(string cur, string targetname)
        {
            RecursivelyCopyDir(cur, targetname);
        }

        private void HandleCleanup(string cur, string TargetDir)
        {
            RemoveOldFilesFromBackup(cur, TargetDir);
        }

        private void RemoveOldFilesFromBackup(string originalDir, string backupHoldingDir)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(backupHoldingDir);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(originalDir, file.Name);
                try
                {
                    if (!File.Exists(temppath))
                    {
                        ReportWithIndentation( "Removing " + file.FullName, TextReporter.TextType.ForLogOnly);
                        DeleteFile(file.FullName);
                    }
                }
                catch (Exception excep)
                {
                    TextReporter.ReportDeleteError(
                        Indentation + "Error deleting " + file.FullName + "\n" + Indentation +
                        excep.Message, file.FullName, excep);
                }
            }

            // If copying subdirectories, copy them and their contents to new location. 
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(originalDir, subdir.Name);
                if (!Directory.Exists(temppath))
                {
                    ReportWithIndentation( "Removing " + subdir.FullName, TextReporter.TextType.ForLogOnly);
                    DeleteDirectory(subdir.FullName);
                }
                else
                {
                    RemoveOldFilesFromBackup(temppath, subdir.FullName);
                }
            }
        }


        private void RecursivelyCopyDir(string sourceDirName, string destDirName)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                ReportWithIndentation(
                     "Source directory does not exist or could not be found: " + sourceDirName,
                    TextReporter.TextType.BackupError);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                try
                {
                    ReportWithIndentation( "Creating directory " + destDirName,
                        TextReporter.TextType.ForLogOnly);
                    CreateDirectory(destDirName);
                }
                catch (Exception excep)
                {
                    ReportWithIndentation(
                         "Error creating directory " + destDirName + "\n" + Indentation + excep.Message,
                        TextReporter.TextType.BackupError);
                }
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                try
                {
                    _currentProgress += file.Length;
                    _overallProgress += file.Length;
                    TextReporter.ReportProgressToUI(_currentProgress, _currentMax, _overallProgress, _overallMax);
                    Tuple<CopyReason, Tuple<DateTime, DateTime>> copyReason = NeedToOverwrite(file, temppath);
                    if (copyReason.Item1 != CopyReason.ShouldNotCopy)
                    {
                        String copyExplanation = "";
                        switch (copyReason.Item1)
                        {
                            case CopyReason.MoreRecent:
                                copyExplanation = "Source targetFile was more recent - " + copyReason.Item2.Item1 + "\t" +
                                                  copyReason.Item2.Item2;
                                break;
                            case CopyReason.DidNotExist:
                                copyExplanation = "File did not exist in source";
                                break;
                        }
                        ReportWithIndentation(
                             "Copying targetFile " + temppath + "\tExplanation: " + copyExplanation,
                            TextReporter.TextType.ForLogOnly);
                        FileCopy(file, temppath);
                    }
                }
                catch (Exception excep)
                {
                    String longMessage = Indentation + "Error copying " + file.FullName + " to " + temppath + "\n" +
                                         Indentation +
                                         excep.Message;
                    TextReporter.ReportCommonCopyError(longMessage, file.FullName, temppath + "\n", excep);
                }
            }

            // If copying subdirectories, copy them and their contents to new location. 
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                try
                {
                    RecursivelyCopyDir(subdir.FullName, temppath);
                }
                catch (Exception)
                {
                    //we want to keep the backup going no matter what 
                }
            }
        }

        private void FileCopy(FileInfo targetFile, string destination)
        {
            if (_debugOnly)
            {
                ReportWithIndentation("Would copy " + targetFile.FullName + " to " + destination,
                    TextReporter.TextType.Output);
            }
            else
            {
                targetFile.CopyTo(destination, true);
            }
        }

        private void CreateDirectory(string destDirName)
        {
            if (_debugOnly)
            {
                ReportWithIndentation("Would created " + destDirName, TextReporter.TextType.Output);
            }
            else
            {
                Directory.CreateDirectory(destDirName);
            }
        }

        private void ReportWithIndentation(string text, TextReporter.TextType output)
        {
            TextReporter.Report(Indentation+text, output);

        }

        /// <returns>
        ///     Description of reason, followed by a tuple containing the time of the source targetFile and the time of the
        ///     targetFile in the backup dir
        /// </returns>
        private Tuple<CopyReason, Tuple<DateTime, DateTime>> NeedToOverwrite(FileInfo file, string temppath)
        {
            if (!File.Exists(temppath))
            {
                return new Tuple<CopyReason, Tuple<DateTime, DateTime>>(
                    CopyReason.DidNotExist, null);
            }
            var existingfile = new FileInfo(temppath);
            if (file.LastWriteTime > existingfile.LastWriteTime)
            {
                return new Tuple<CopyReason, Tuple<DateTime, DateTime>>(
                    CopyReason.MoreRecent, new Tuple<DateTime, DateTime>(file.LastWriteTime, existingfile.LastWriteTime));
            }
            return new Tuple<CopyReason, Tuple<DateTime, DateTime>>(CopyReason.ShouldNotCopy, null);
        }

        private bool AllTargetFilesExist(BackupPattern currentBackupPattern)
        {
            foreach (Source cur in currentBackupPattern.Pattern.Keys)
            {
                if (!Directory.Exists(cur.BackupSource))
                {
                    ReportWithIndentation( "Error: " + cur.BackupSource + " did not exist.",
                        TextReporter.TextType.BackupError);
                    return false;
                }
            }
            return true;
        }

        private enum CopyReason
        {
            ShouldNotCopy,
            DidNotExist,
            MoreRecent
        }
    }
}