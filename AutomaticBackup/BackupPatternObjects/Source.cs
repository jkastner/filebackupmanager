using System;

namespace AutomaticBackup
{
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