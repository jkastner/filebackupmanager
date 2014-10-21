using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBackupConsole
{
    public static class PathConstants
    {

        private static string _currentDirectory = "";
        public static string CurrentDirectory
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_currentDirectory))
                {
                    _currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                return _currentDirectory;
            }
        }
    }
}
