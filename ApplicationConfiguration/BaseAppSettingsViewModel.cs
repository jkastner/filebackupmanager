using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using ApplicationConfiguration.Annotations;

namespace ApplicationConfiguration
{
    public abstract class BaseAppSettingsViewModel : INotifyPropertyChanged
    {
        private readonly Configuration _configuration;

        private readonly Dictionary<Type, Action<String>> _registeredParsers = new Dictionary<Type, Action<String>>();

        public BaseAppSettingsViewModel()
        {
            _configuration = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
        }

        public IEnumerable<Type> ValidTypes
        {
            get
            {
                yield return typeof (bool);
                yield return typeof (int);
                yield return typeof(string);
                foreach (var cur in _registeredParsers)
                {
                    yield return cur.Key;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Any types that can be parsed in addition to bool and int (such as a DateTime property)
        /// </summary>
        public void AddValidType(Type targetType, Action<String> theParser)
        {
            if (!_registeredParsers.ContainsKey(targetType))
            {
                _registeredParsers.Add(targetType, null);
            }
            _registeredParsers[targetType] = theParser;
        }

        /// <summary>
        /// Used to load values that need to be changes after default loading. Called after the values are populated in the constructor.
        /// </summary>
        protected virtual void LoadOverriddenValues()
        {
        }

        protected void LoadAppConfigInfo()
        {
            //From
            //http://stackoverflow.com/questions/824802/c-how-to-get-all-public-both-get-and-set-string-properties-of-a-type
            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo p in properties)
            {
                if (!ValidTypes.Contains(p.PropertyType))
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
                    _registeredParsers[p.PropertyType].Invoke(readValue);
                }
            }
        }

        private void SaveChanges()
        {
            //From
            //http://stackoverflow.com/questions/824802/c-how-to-get-all-public-both-get-and-set-string-properties-of-a-type
            PropertyInfo[] properties =
                GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo p in properties)
            {
                if (!ValidTypes.Contains(p.PropertyType))
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
                if (
                    typeof (IEnumerable).IsAssignableFrom(p.PropertyType)
                    && (p.PropertyType != typeof(String))
                    )
                {
                    var curProp = p.GetValue(this) as IEnumerable;
                    var sb = new List<string>();
                    foreach (object cur in curProp)
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


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}