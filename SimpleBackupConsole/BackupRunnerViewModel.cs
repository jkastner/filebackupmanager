using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace SimpleBackupConsole
{
    public class BackupRunnerViewModel : INotifyPropertyChanged
    {
        private static BackupRunnerViewModel _instance;
        private BackupPattern _currentPattern;
        private bool _shouldWriteLog = true;

        private BackupRunnerViewModel()
        {
            
        }

        public BackupPattern CurrentBackupPattern
        {
            get { return _currentPattern; }
            set
            {
                _currentPattern = value;
                OnBackupPatternDescriptionChanged(null);
            }
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




        public bool CloseUIOnCompleted { get; set; }

        public String FullBackupPatternDescription
        {
            get
            {
                if (_currentPattern == null)
                {
                    return "Please select a backup pattern.";
                }
                var sb = new StringBuilder();
                sb.Append("\nBackup:");
                foreach (Source curSource in _currentPattern.Sources)
                {
                    sb.Append("\nFrom: " + curSource.BackupSource);
                    foreach (Destination curDestination in _currentPattern.Pattern[curSource])
                    {
                        String finalUnique = _currentPattern.UniqueFinalPath(curSource, curDestination, ConfigViewModel.Instance.StaggerBackup);
                        sb.Append("\n\tTo:  " + finalUnique);
                    }
                }

                return sb.ToString();
            }
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