using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBackupConsole
{
    public class BackupPattern
    {
        public string Description { get; set; }
        private Dictionary<Source, List<Destination>> _pattern = new Dictionary<Source, List<Destination>>();
        public Dictionary<Source, List<Destination>> Pattern
        {
            get { return _pattern; }
            private set { _pattern = value; }
        }

        public List<Source> Sources
        {
            get { return Pattern.Keys.ToList(); }
        }
        public IEnumerable<Destination> Destinations
        {
            get
            {
                HashSet<Destination> ret = new HashSet<Destination>();
                foreach (var cur in Pattern)
                {
                    foreach (var curdest in cur.Value)
                    {
                        ret.Add(curdest);
                    }
                }
                return ret;
            }
        }

        public BackupPattern(String description)
        {
            Description = description;
        }

        public void AddBackup(Source source, Destination dest)
        {
            if (!Pattern.ContainsKey(source))
            {
                Pattern.Add(source, new List<Destination>());
            }
            Pattern[source].Add(dest);
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
