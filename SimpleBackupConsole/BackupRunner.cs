using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;

namespace SimpleBackupConsole
{
    public class BackupRunner
    {
        private static BackupRunner _instance;

        public List<String> DirectoriesToBackup = new List<String>();
        public List<String> DirectoriesToHoldBackups = new List<String>();

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

        private long _currentProgress, _overallProgress, _currentMax, _overallMax;
        public void StartBackup()
        {
            if (DateTime.Now.Date.DayOfWeek != BackupRunnerViewModel.Instance.TargetDay)
            {
                BackupRunnerViewModel.Instance.CloseUIOnCompleted = true;
                TextReporter.Report(
                    "Target day " + BackupRunnerViewModel.Instance.TargetDay.ToString() + " did not match current day " +
                    DateTime.Now.DayOfWeek, TextReporter.TextType.Output);
                BackupRunnerViewModel.Instance.ShouldWriteLog = false;
                BackupRunnerViewModel.Instance.OnBackupCompleted(new EventArgs());
                return;
            }
            if (!DirectoriesToBackup.Any())
            {
                BackupRunnerViewModel.Instance.OnBackupCompleted(new EventArgs());
                return;
            }
            if (!AllTargetFilesExist())
            {
                BackupRunnerViewModel.Instance.OnBackupCompleted(new EventArgs());
                return;
            }
            DateTime startTime = DateTime.Now;

            TextReporter.Report("Backup started at "+DateTime.Now.ToString(), TextReporter.TextType.Output);
            TextReporter.Report("Estimating copy size...", TextReporter.TextType.Output);
            Dictionary<String, long> directorySizes = GetAllDirectorySizes();
            _overallMax = directorySizes.Sum(x => x.Value);
            _overallMax *= DirectoriesToHoldBackups.Count;
            for (int index = 0; index < DirectoriesToHoldBackups.Count; index++)
            {
                _tabIndex++;
                string BaseTargetDir = DirectoriesToHoldBackups[index];
                if (BackupRunnerViewModel.Instance.StaggerBackup)
                {
                    if (DateTime.Now.Day%2 == 0)
                    {
                        BaseTargetDir = BaseTargetDir + "_2";
                    }
                }
                TextReporter.Report("Starting backup in: " + BaseTargetDir, TextReporter.TextType.Output);
                try
                {
                    Thread.Sleep(2000);
                    TextReporter.Report(Indentation + "Creating " + BaseTargetDir, TextReporter.TextType.Output);
                    if (!Directory.Exists(BaseTargetDir))
                    {
                        Directory.CreateDirectory(BaseTargetDir);
                    }
                    foreach (string cur in DirectoriesToBackup)
                    {
                        try
                        {
                            _currentMax = directorySizes[cur];
                            _currentProgress = 0;
                            string TargetDir = BaseTargetDir + "\\" + Path.GetFileName(cur);
                            TargetDir = UniqueName(cur, TargetDir);
                            _tabIndex++;
                            TextReporter.Report(Indentation + "Copying " + cur + " to " + TargetDir,
                                                TextReporter.TextType.Output);
                            _tabIndex++;
                            TextReporter.Report(Indentation + "Copying files...", TextReporter.TextType.Output);
                            HandleBackup(cur, TargetDir);
                            TextReporter.Report(Indentation + "Cleaning old files from "+TargetDir, TextReporter.TextType.Output);
                            HandleCleanup(cur, TargetDir);
                            _tabIndex--;
                            TextReporter.Report(Indentation + "Completed copy of " + cur, TextReporter.TextType.Output);
                        }
                        catch (Exception e)
                        {
                            _tabIndex++;
                            TextReporter.Report(
                                Indentation + "ERROR: Could not copy " + cur + " to " + BaseTargetDir + "\n" +
                                Indentation + "Exception: " + e.Message, TextReporter.TextType.BackupError);
                            _tabIndex--;
                        }
                        _tabIndex--;
                    }
                    
                }
                catch (Exception e)
                {
                    TextReporter.Report(Indentation + "ERROR: " + e.Message, TextReporter.TextType.BackupError);
                }
                _tabIndex--;
            }
            if (BackupRunnerViewModel.Instance.ShutdownOnCompletion)
            {
                BackupRunnerViewModel.Instance.OnShutdownRequested(new EventArgs());
                TextReporter.Report("Shutting down...", TextReporter.TextType.Output);
            }
            DateTime endTime = DateTime.Now;
            var backupDuration = Math.Round((endTime - startTime).TotalMinutes, 1);
            TextReporter.Report("Backup ended at " + DateTime.Now.ToString(), TextReporter.TextType.Output);
            TextReporter.Report("Duration:  " + backupDuration + " minutes", TextReporter.TextType.Output);
            BackupRunnerViewModel.Instance.OnBackupCompleted(new EventArgs());
        }

        private String UniqueName(string currentSource, string currentTarget)
        {
            var equalNames =
                DirectoriesToBackup.Where(
                    x => Path.GetFileName(x).Equals(Path.GetFileName(currentTarget), StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (equalNames.Count > 1)
            {

                var ret = Directory.GetParent(currentTarget) + "\\" + Directory.GetParent(currentSource).Name + "_" + Path.GetFileName(currentTarget);
                return ret;
            }
            return currentTarget;
        }



        private Dictionary<string, long> GetAllDirectorySizes()
        {
            Dictionary<String, long> info = new Dictionary<string, long>();
            foreach (var cur in DirectoriesToBackup)
            {
                if (!info.ContainsKey(cur))
                {
                    info.Add(cur, GetDirectorySize(new DirectoryInfo(cur)));
                }
            }
            return info;
        }

        /// <summary>
        /// Directory.delete stops if even a single file is write-protected.
        /// Use this helper method instead
        /// </summary>
        /// <param name="target_dir"></param>
        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    TextReporter.Report("Could not delete file: "+file, TextReporter.TextType.BackupError);
                }
            }
            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }
            try
            {
                Directory.Delete(target_dir, false);
            }
            catch (Exception)
            {
                TextReporter.Report("Could not delete directory: " + target_dir, TextReporter.TextType.BackupError);
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
                        TextReporter.Report(Indentation+"Removing "+file.FullName, TextReporter.TextType.ForLogOnly);
                        file.Delete();
                    }
                }
                catch (Exception excep)
                {
                    TextReporter.Report(
                        Indentation + "Error deleting " + file.FullName + "\n" + Indentation +
                        excep.Message, TextReporter.TextType.BackupError);
                }
            }

            // If copying subdirectories, copy them and their contents to new location. 
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(originalDir, subdir.Name);
                if (!Directory.Exists(temppath))
                {
                    TextReporter.Report(Indentation+"Removing " + subdir.FullName, TextReporter.TextType.ForLogOnly);
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
                TextReporter.Report(
                    Indentation + "Source directory does not exist or could not be found: " + sourceDirName,
                    TextReporter.TextType.BackupError);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                try
                {
                    TextReporter.Report(Indentation+ "Creating directory " + destDirName, TextReporter.TextType.ForLogOnly);
                    Directory.CreateDirectory(destDirName);
                }
                catch (Exception excep)
                {
                    TextReporter.Report(
                        Indentation + "Error creating directory " + destDirName + "\n" + Indentation + excep.Message,
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
                    var copyReason = NeedToOverwrite(file, temppath);
                    if(copyReason.Item1!=CopyReason.ShouldNotCopy)
                    {
                        String copyExplanation = "";
                        switch (copyReason.Item1)
                        {
                            case CopyReason.MoreRecent:
                                copyExplanation = "Source file was more recent - " + copyReason.Item2.Item1 + "\t" +
                                                  copyReason.Item2.Item2;
                                break;
                            case CopyReason.DidNotExist:
                                copyExplanation = "File did not exist in source";
                                break;
                        }
                        TextReporter.Report(Indentation + "Copying file " + temppath+"\tExplanation: "+copyExplanation, TextReporter.TextType.ForLogOnly);
                        file.CopyTo(temppath, true);
                    }
                }
                catch (Exception excep)
                {
                    TextReporter.Report(
                        Indentation + "Error copying " + file.FullName + " to " + temppath + "\n" + Indentation +
                        excep.Message, TextReporter.TextType.BackupError);
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

        private enum CopyReason
        {
            ShouldNotCopy, DidNotExist, MoreRecent
        }
        /// <returns>Description of reason, followed by a tuple containing the time of the source file and the time of the 
        /// file in the backup dir
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
            return new Tuple<CopyReason, Tuple<DateTime, DateTime>>(CopyReason.ShouldNotCopy, null) ;
        }

        private bool AllTargetFilesExist()
        {
            foreach (string cur in DirectoriesToBackup)
            {
                if (!Directory.Exists(cur))
                {
                    TextReporter.Report(Indentation + "Error: " + cur + " did not exist.",
                                        TextReporter.TextType.BackupError);
                    return false;
                }
            }
            return true;
        }
    }
}