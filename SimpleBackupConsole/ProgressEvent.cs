using System;

namespace SimpleBackupConsole
{
    public class ProgressEvent : EventArgs
    {
        public enum ProgressType
        {
            CurrentDirectory,
            Main
        }

        public ProgressEvent(long currentProgress, long currentMax, long overallProgress, long overallMax)
        {
            CurrentProgress = currentProgress;
            CurrentMax = currentMax;
            OverallProgress = overallProgress;
            OverallMax = overallMax;
        }


        public long CurrentProgress { get; set; }
        public long OverallProgress { get; set; }
        public long CurrentMax { get; set; }
        public long OverallMax { get; set; }
    }
}