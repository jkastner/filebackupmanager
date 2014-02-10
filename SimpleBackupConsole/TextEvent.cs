using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBackupConsole
{
    public class TextEvent : EventArgs
    {
        public String Text { get; set; }
        public TextReporter.TextType ReportType { get; set; }
        public TextEvent(String text, TextReporter.TextType reportType)
        {
            Text = text;
            ReportType = reportType;
        }
    }
}
