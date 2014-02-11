using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBackupConsole
{
    public class BackupPattern
    {
        private List<Tuple<Source, Destination>> _pattern = new List<Tuple<Source, Destination>>();

        public List<Tuple<Source, Destination>> Pattern
        {
            get { return _pattern; }
            set { _pattern = value; }
        }
        
    }

    public class Destination
    {
        public String BackupDestination { get; set; }
        public Destination(String location)
        {
            BackupDestination = location;
        }
    }

    public class Source
    {
        public String BackupSource { get; set; }
        public Source(String location)
        {
            BackupSource = location;
        }
    }
}
