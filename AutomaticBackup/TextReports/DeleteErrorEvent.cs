using System;

namespace AutomaticBackup
{
    public class DeleteErrorEvent : TextEvent, ICommonError
    {
        public string TargetFile { get; set; }
        public Exception ThrownException { get; set; }


        public DeleteErrorEvent(String longMessage, TextReporter.TextType reportType, string targetFile,
            Exception thrownException) :
                base(longMessage, reportType)
        {
            TargetFile = targetFile;
            ThrownException = thrownException;
        }

        public string ShortDescription
        {
            get
            {
                return TargetFile;
            }
        }
    }
}