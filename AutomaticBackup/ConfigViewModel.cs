using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;


namespace AutomaticBackup
{
    public class ConfigViewModel : BaseAppSettingsViewModel
    {

        private static ConfigViewModel _instance;
        private bool _closeWindowOnCompletion;
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
            AddValidType(typeof(HashSet<DayOfWeek>), DayOfWeekParser);
            LoadAppConfigInfo();
            LoadOverriddenValues();
        }

        private void DayOfWeekParser(string readValue)
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
            SetCheckboxesBySet(TargetDays);
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
                ChangeTargetDaysByBool(value, DayOfWeek.Sunday);
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
                ChangeTargetDaysByBool(value, DayOfWeek.Monday);
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
                ChangeTargetDaysByBool(value, DayOfWeek.Tuesday);
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
                ChangeTargetDaysByBool(value, DayOfWeek.Wednesday);
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
                ChangeTargetDaysByBool(value, DayOfWeek.Thursday);
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
                ChangeTargetDaysByBool(value, DayOfWeek.Friday);
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
                ChangeTargetDaysByBool(value, DayOfWeek.Saturday);
            
            }
        }

        private bool _calculateCopyTime = true;

        public bool CalculateCopyTime
        {
            get { return _calculateCopyTime; }
            set
            {
                _calculateCopyTime = value;
                OnPropertyChanged();
            }
        }
        


        private void ChangeTargetDaysByBool(bool newVal, DayOfWeek day)
        {
            if (newVal)
            {
                TargetDays.Add(day);
            }
            else
            {
                TargetDays.Remove(day);
            }

        }

        private void SetCheckboxesBySet(HashSet<DayOfWeek> set)
        {
            SundayChecked = set.Contains(DayOfWeek.Sunday);
            MondayChecked = set.Contains(DayOfWeek.Monday);
            TuesdayChecked = set.Contains(DayOfWeek.Tuesday);
            WednesdayChecked = set.Contains(DayOfWeek.Wednesday);
            ThursdayChecked = set.Contains(DayOfWeek.Thursday);
            FridayChecked = set.Contains(DayOfWeek.Friday);
            SaturdayChecked = set.Contains(DayOfWeek.Saturday);
        }



        public void ToggleAllDays()
        {
            if (TargetDays.Any())
            {
                TargetDays.Clear();
                SetCheckboxesBySet(TargetDays);
            }
            else
            {
                SundayChecked = true;
                MondayChecked = true;
                TuesdayChecked = true;
                WednesdayChecked = true;
                ThursdayChecked = true;
                FridayChecked = true;
                SaturdayChecked = true;
            }
        }
    }
}