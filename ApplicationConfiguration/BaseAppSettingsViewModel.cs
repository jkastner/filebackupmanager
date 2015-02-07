using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ApplicationConfiguration.Annotations;


namespace SimpleBackupConsole
{
    public abstract class BaseAppSettingsViewModel : INotifyPropertyChanged
    {
        private readonly Configuration _configuration;
        private HashSet<Type> _validTypes;
        private bool _yay;

        internal BaseAppSettingsViewModel()
        {
            _configuration = ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.ExecutablePath);
            _validTypes = DefineValidTypes();
            LoadAppConfigInfo();
            LoadOverriddenValues();
        }

        protected abstract HashSet<Type> DefineValidTypes();

        protected abstract void LoadOverriddenValues();

        private void LoadAppConfigInfo()
        {
            //From
            //http://stackoverflow.com/questions/824802/c-how-to-get-all-public-both-get-and-set-string-properties-of-a-type
            PropertyInfo[] properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo p in properties)
            {
                if (!_validTypes.Contains(p.PropertyType))
                {
                    continue;
                }
                // If not writable then cannot null it; if not readable then cannot check it's value
                if (!p.CanWrite || !p.CanRead)
                {
                    continue;
                }

                MethodInfo mget = p.GetGetMethod(false);
                // Get method has to be public
                if (mget == null)
                {
                    continue;
                }
                string readValue = ConfigurationManager.AppSettings[p.Name];
                if (readValue == null)
                {
                    continue;
                }
                if (p.PropertyType == typeof (bool))
                {
                    p.SetValue(this, bool.Parse(readValue));
                }
                else if (p.PropertyType == typeof (int))
                {
                    p.SetValue(this, int.Parse(readValue));
                }
                else if (p.PropertyType == typeof (string))
                {
                    p.SetValue(this, readValue);
                }
                else
                {
                    UseCustomParser(p, readValue);
                }
            }
        }

        protected virtual void UseCustomParser(PropertyInfo propertyInfo, string readValue)
        {
            
        }

        private void SaveChanges()
        {
            //From
            //http://stackoverflow.com/questions/824802/c-how-to-get-all-public-both-get-and-set-string-properties-of-a-type
            PropertyInfo[] properties =
                this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo p in properties)
            {
                if (!_validTypes.Contains(p.PropertyType))
                {
                    continue;
                }
                // If not writable then cannot null it; if not readable then cannot check it's value
                if (!p.CanWrite || !p.CanRead)
                {
                    continue;
                }

                MethodInfo mget = p.GetGetMethod(false);
                // Get method has to be public
                if (mget == null)
                {
                    continue;
                }
                _configuration.AppSettings.Settings.Remove(p.Name);
                if (typeof (IEnumerable).IsAssignableFrom(p.PropertyType))
                {
                    var curProp = p.GetValue(this) as IEnumerable;
                    List<String> sb = new List<string>();
                    foreach (var cur in curProp)
                    {
                        sb.Add(cur.ToString());
                    }
                    _configuration.AppSettings.Settings.Add(p.Name, string.Join(",", sb.ToArray()));
                }
                else
                {
                    _configuration.AppSettings.Settings.Add(p.Name, p.GetValue(this).ToString());
                }

            }

            _configuration.Save(ConfigurationSaveMode.Modified);
        }

        ~BaseAppSettingsViewModel()
        {
            SaveChanges();
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