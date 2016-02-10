using System;

namespace AutomaticBackup
{
    class DeleteErrorEvent : TextEvent
    {
        public string SourceFile { get; set; }
        public string Destinationfile { get; set; }
        public string TargetFile { get; set; }
        public Exception ThrownException { get; set; }


        public DeleteErrorEvent(String longMessage, TextReporter.TextType reportType, string targetFile,
            Exception thrownException) :
                base(longMessage, reportType)
        {
            TargetFile = targetFile;
            ThrownException = thrownException;
        }
    }
}