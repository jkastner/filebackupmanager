using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleBackupConsole
{
    public class BackupPattern
    {
        private Dictionary<Source, HashSet<Destination>> _pattern = new Dictionary<Source, HashSet<Destination>>();

        public BackupPattern(String description)
        {
            Description = description;
        }

        public string Description { get; set; }

        public Dictionary<Source, HashSet<Destination>> Pattern
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
                var ret = new HashSet<Destination>();
                foreach (var cur in Pattern)
                {
                    foreach (Destination curdest in cur.Value)
                    {
                        ret.Add(curdest);
                    }
                }
                return ret;
            }
        }

        public void AddBackup(Source source, Destination dest)
        {
            if (!Pattern.ContainsKey(source))
            {
                Pattern.Add(source, new HashSet<Destination>());
            }
            Pattern[source].Add(dest);
        }

        public String UniqueFinalPath(Source source, Destination dest)
        {
            //was changed, new description
            bool anyMatch = false;
            var allDestinations = new List<string>();
            String staggerString = "";
            bool staggerBackup = BackupRunnerViewModel.Instance.StaggerBackup;
            if (staggerBackup && BackupRunnerViewModel.Instance.RunAutomatically)
            {
                if (DateTime.Now.Day%2 != 0)
                {
                    staggerBackup = false;
                }
            }

            if (staggerBackup)
            {
                staggerString = "_2";
            }
            foreach (var curPair in Pattern)
            {
                Source curSource = curPair.Key;
                foreach (Destination curDest in curPair.Value)
                {
                    if (curSource.Equals(source) && curDest.Equals(dest))
                    {
                        continue;
                    }
                    String curPath = curDest.BackupDestination + staggerString + "\\" +
                                     Path.GetFileName(curSource.BackupSource);
                    allDestinations.Add(curPath);
                }
            }
            string thisFinalPath = dest.BackupDestination + staggerString + "\\" + Path.GetFileName(source.BackupSource);
            anyMatch = allDestinations.Any(x => x.Equals(thisFinalPath));
            if (anyMatch)
            {
                return dest.BackupDestination + staggerString + "\\" + Directory.GetParent(source.BackupSource).Name +
                       "_" + Path.GetFileName(source.BackupSource);
            }
            return thisFinalPath;
        }
    }

    public class Destination
    {
        public Destination(String location)
        {
            BackupDestination = location;
        }

        public String BackupDestination { get; set; }

        public override bool Equals(object obj)
        {
            var destother = obj as Destination;
            if (destother == null)
            {
                return false;
            }
            return destother.BackupDestination.Equals(BackupDestination);
        }

        public override int GetHashCode()
        {
            return BackupDestination.GetHashCode();
        }
    }

    public class Source
    {
        public Source(String location)
        {
            BackupSource = location;
        }

        public String BackupSource { get; set; }

        public override bool Equals(object obj)
        {
            var sourceother = obj as Source;
            if (sourceother == null)
            {
                return false;
            }
            return sourceother.BackupSource.Equals(BackupSource);
        }

        public override int GetHashCode()
        {
            return BackupSource.GetHashCode();
        }
    }
}