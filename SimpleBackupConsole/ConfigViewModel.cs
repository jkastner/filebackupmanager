using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SimpleBackupConsole
{
    public class ConfigViewModel : BaseAppSettingsViewModel
    {
        private static ConfigViewModel _instance;
        private bool _closeWindowOnCompletion;
        private bool _runAutomatically = false;
        private bool _shutdownComputerOnCompletion;
        private bool _staggerBackup = true;
        private HashSet<DayOfWeek> _targetDays = new HashSet<DayOfWeek>();
        private bool _sundayChecked;
        private bool _mondayChecked;
        private bool _tuesdayChecked;
        private bool _wednesdayChecked;
        private bool _thursdayChecked;
        private bool _fridayChecked;
        private bool _saturdayChecked;

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

        public bool SundayChecked
        {
            get { return _sundayChecked; }
            set
            {
                if (value.Equals(_sundayChecked)) return;
                _sundayChecked = value;
                OnPropertyChanged();
                SetupDays();
            }
        }

        public bool MondayChecked
        {
            get { return _mondayChecked; }
            set
            {
                if (value.Equals(_mondayChecked)) return;
                _mondayChecked = value;
                OnPropertyChanged();
                SetupDays();
            }
        }

        public bool TuesdayChecked
        {
            get { return _tuesdayChecked; }
            set
            {
                if (value.Equals(_tuesdayChecked)) return;
                _tuesdayChecked = value;
                OnPropertyChanged();
                SetupDays();
            }
        }

        public bool WednesdayChecked
        {
            get { return _wednesdayChecked; }
            set
            {
                if (value.Equals(_wednesdayChecked)) return;
                _wednesdayChecked = value;
                OnPropertyChanged();
                SetupDays();
            }
        }

        public bool ThursdayChecked
        {
            get { return _thursdayChecked; }
            set
            {
                if (value.Equals(_thursdayChecked)) return;
                _thursdayChecked = value;
                OnPropertyChanged();
                SetupDays();
            }
        }

        public bool FridayChecked
        {
            get { return _fridayChecked; }
            set
            {
                if (value.Equals(_fridayChecked)) return;
                _fridayChecked = value;
                OnPropertyChanged();
                SetupDays();
            }
        }

        public bool SaturdayChecked
        {
            get { return _saturdayChecked; }
            set
            {
                if (value.Equals(_saturdayChecked)) return;
                _saturdayChecked = value;
                OnPropertyChanged();
                SetupDays();
            }
        }

        private void SetupDays()
        {
            TargetDays.Clear();
            if (SundayChecked)
            {
                TargetDays.Add(DayOfWeek.Sunday);
            }
            if (MondayChecked)
            {
                TargetDays.Add(DayOfWeek.Monday);
            }
            if (TuesdayChecked)
            {
                TargetDays.Add(DayOfWeek.Tuesday);
            }
            if (WednesdayChecked)
            {
                TargetDays.Add(DayOfWeek.Wednesday);
            }
            if (ThursdayChecked)
            {
                TargetDays.Add(DayOfWeek.Thursday);
            }
            if (FridayChecked)
            {
                TargetDays.Add(DayOfWeek.Friday);
            }
            if (SaturdayChecked)
            {
                TargetDays.Add(DayOfWeek.Saturday);
            }
        }

        protected override void UseCustomParser(PropertyInfo propertyInfo, string readValue)
        {
            if (propertyInfo.PropertyType == typeof (HashSet<DayOfWeek>))
            {
                string[] set = readValue.Split(',');
                TargetDays.Clear();
                foreach (string cur in set)
                {
                    Enum q;
                    DayOfWeek day;
                    if (Enum.TryParse(cur, out day))
                    {
                        TargetDays.Add(day);
                    }
                }
                if (TargetDays.Contains(DayOfWeek.Sunday))
                    SundayChecked = true;
                if (TargetDays.Contains(DayOfWeek.Monday))
                    MondayChecked = true;
                if (TargetDays.Contains(DayOfWeek.Tuesday))
                    TuesdayChecked = true;
                if (TargetDays.Contains(DayOfWeek.Wednesday))
                    WednesdayChecked = true;
                if (TargetDays.Contains(DayOfWeek.Thursday))
                    ThursdayChecked = true;
                if (TargetDays.Contains(DayOfWeek.Friday))
                    FridayChecked = true;
                if (TargetDays.Contains(DayOfWeek.Saturday))
                    SaturdayChecked = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected override HashSet<Type> DefineValidTypes()
        {
            return new HashSet<Type> {typeof (bool), typeof (String), typeof (HashSet<DayOfWeek>)};
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