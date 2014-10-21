using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BackupUI.Annotations;
using SimpleBackupConsole;

namespace BackupUI
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private BackupRunnerViewModel _backupRunnerViewModel = BackupRunnerViewModel.Instance;
        private ConfigViewModel _configViewModel = ConfigViewModel.Instance;

        public BackupRunnerViewModel BackupRunnerViewModel
        {
            get { return _backupRunnerViewModel; }
            set
            {
                if (Equals(value, _backupRunnerViewModel)) return;
                _backupRunnerViewModel = value;
                OnPropertyChanged();
            }
        }

        public ConfigViewModel ConfigViewModel
        {
            get { return _configViewModel; }
            set
            {
                if (Equals(value, _configViewModel)) return;
                _configViewModel = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
