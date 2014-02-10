using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace SimpleBackupConsole
{
    public class BackupRunnerViewModel : INotifyPropertyChanged
    {
        private static BackupRunnerViewModel _instance;
        private bool _runAutomatically = true;
        private bool _shouldWriteLog = true;
        private bool _shutdownOnCompletion;
        private bool _staggerBackup = true;
        private DayOfWeek _targetDay = DateTime.Now.DayOfWeek;

        private BackupRunnerViewModel()
        {
        }

        public static BackupRunnerViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BackupRunnerViewModel();
                }
                return _instance;
            }
        }

        public bool ShutdownOnCompletion
        {
            get { return _shutdownOnCompletion; }
            set
            {
                _shutdownOnCompletion = value;
                OnPropertyChanged("ShutdownOnCompletion");
            }
        }

        public bool StaggerBackup
        {
            get { return _staggerBackup; }
            set
            {
                _staggerBackup = value;
                OnPropertyChanged("StaggerBackup");
                OnBackupPatternDescriptionChanged(new EventArgs());
            }
        }

        public bool RunAutomatically
        {
            get { return _runAutomatically; }
            set
            {
                _runAutomatically = value;
                OnPropertyChanged("RunAutomatically");
            }
        }

        public DayOfWeek TargetDay
        {
            get { return _targetDay; }
            set
            {
                _targetDay = value;
                OnPropertyChanged("TargetDay");
            }
        }

        public bool CloseUIOnCompleted { get; set; }

        public String FullBackupPatternDescription
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append("\nBackup:");
                foreach (string curSource in BackupRunner.Instance.DirectoriesToBackup)
                {
                    sb.Append("\nFrom: " + curSource);
                    foreach (string curDestination in BackupRunner.Instance.DirectoriesToHoldBackups)
                    {
                        String destinationWithStagger = curDestination;
                        if (StaggerBackup)
                        {
                            destinationWithStagger = curDestination + "_2";
                        }

                        sb.Append("\n\tTo: " + destinationWithStagger + "\\" + Path.GetFileName(curSource));
                    }
                }
                var backupDirNameUniqueness = new Dictionary<String, String>();
                foreach (string curSource in BackupRunner.Instance.DirectoriesToBackup)
                {
                    if (!Directory.Exists(curSource))
                    {
                        sb.Append("\nWARNING: " + curSource + " does not exist.");
                    }
                    if (backupDirNameUniqueness.ContainsKey(Path.GetFileName(curSource)))
                    {
                        var uniqueNames = BackupRunnerViewModel.Instance.GetUniqueNameAttempt(
                            backupDirNameUniqueness[Path.GetFileName(curSource)], curSource);
                        sb.Append("\nWARNING: both\n\t" + backupDirNameUniqueness[Path.GetFileName(curSource)] +
                                  "\n\tand\n\t" + curSource +
                                  " have the same backup name. They will be renamed in the backup to:\n\t"+uniqueNames.Item1+"\n\tand\n\t"+uniqueNames.Item2);
                        

                    }
                    else
                    {
                        backupDirNameUniqueness.Add(Path.GetFileName(curSource), curSource);
                    }
                }
                foreach (string curDest in BackupRunner.Instance.DirectoriesToHoldBackups)
                {
                    string parentName = Directory.GetParent(curDest).FullName;
                    if (!Directory.Exists(curDest))
                    {
                        sb.Append("\nWARNING: " + parentName + " does not exist.");
                    }
                }

                return sb.ToString();
            }
        }

        internal Tuple<String, String> GetUniqueNameAttempt(string path1, string path2)
        {
            var curParent1 = Directory.GetParent(path1);
            var curParent2 = Directory.GetParent(path2);
            var curName1 = Path.GetFileName(path1);
            var curName2 = Path.GetFileName(path2);

            curName1 = curParent1.Name +"_"+ curName1;
            curName2 = curParent2.Name + "_" + curName2;
            Tuple<String, String> uniqueNames = new Tuple<string, string>(curName1, curName2);
            return uniqueNames;
        }



        public bool ShouldWriteLog
        {
            get { return _shouldWriteLog; }
            set { _shouldWriteLog = value; }
        }


        public event EventHandler BackupPatternDescriptionChanged;

        public virtual void OnBackupPatternDescriptionChanged(EventArgs e)
        {
            if (BackupPatternDescriptionChanged != null)
            {
                BackupPatternDescriptionChanged(this, e);
            }
        }

        public event EventHandler ShutdownRequested;

        public virtual void OnShutdownRequested(EventArgs e)
        {
            if (ShutdownRequested != null)
            {
                ShutdownRequested(this, e);
            }
        }

        public event EventHandler BackupCompleted;

        public virtual void OnBackupCompleted(EventArgs e)
        {
            if (BackupCompleted != null)
            {
                BackupCompleted(this, e);
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}