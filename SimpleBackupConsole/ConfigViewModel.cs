using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace SimpleBackupConsole
{
    public class ConfigViewModel : BaseAppSettingsViewModel
    {
        private static ConfigViewModel _instance;
        private bool _runAutomatically = false;
        private bool _shutdownComputerOnCompletion;
        private bool _staggerBackup = true;
        private HashSet<DayOfWeek> _targetDays = new HashSet<DayOfWeek>();
        private bool _closeWindowOnCompletion;

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

        public HashSet<DayOfWeek> TargetDays
        {
            get { return _targetDays; }
            set
            {
                _targetDays = value;
                OnPropertyChanged();
            }
        }


        public bool ShutdownComputerOnCompletion
        {
            get { return _shutdownComputerOnCompletion; }
            set
            {
                _shutdownComputerOnCompletion = value;
                OnPropertyChanged();
            }
        }

        public bool CloseWindowOnCompletion
        {
            get { return _closeWindowOnCompletion; }
            set
            {
                if (value.Equals(_closeWindowOnCompletion)) return;
                _closeWindowOnCompletion = value;
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
            private set
            {
                _runAutomatically = value;
                OnPropertyChanged();
            }
        }

        public void AllowAutomaticRun()
        {
            _runAutomatically = true;
        }

        protected override void UseCustomParser(PropertyInfo propertyInfo, string readValue)
        {
            if (propertyInfo.PropertyType == typeof (HashSet<DayOfWeek>))
            {
                var set = readValue.Split(',');
                TargetDays.Clear();
                foreach (var cur in set)
                {
                    Enum q;
                    DayOfWeek day;
                    if (DayOfWeek.TryParse(readValue, out day))
                    {
                        TargetDays.Add(day);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected override HashSet<Type> DefineValidTypes()
        {
            return new HashSet<Type>() { typeof(bool), typeof(String), typeof(HashSet<DayOfWeek>) };
        }

        protected override void LoadOverriddenValues()
        {
            if (!TargetDays.Any())
            {
                TargetDays.Add(DateTime.Now.DayOfWeek);
            }
        }


       


    }
}