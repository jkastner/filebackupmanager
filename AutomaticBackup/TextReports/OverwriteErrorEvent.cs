using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticBackup
{
    public class OverwriteErrorEvent : TextEvent, ICommonError
    {
        public string SourceFile { get; set; }
        public string Destinationfile { get; set; }
        public Exception ThrownException { get; set; }


        public OverwriteErrorEvent(String longMessage, TextReporter.TextType reportType, string sourceFile,
            string destinationfile, Exception thrownException) :
                base(longMessage, reportType)
        {
            SourceFile = sourceFile;
            Destinationfile = destinationfile;
            ThrownException = thrownException;
        }

        public string ShortDescription
        {
            get { return Destinationfile; } 
        }
    }
}
