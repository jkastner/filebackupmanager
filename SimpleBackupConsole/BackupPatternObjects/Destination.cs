using System;

namespace SimpleBackupConsole
{
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
}