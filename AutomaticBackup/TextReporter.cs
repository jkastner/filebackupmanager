using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutomaticBackup
{
    public class TextReporter
    {
        public enum TextType
        {
            InitialError,
            Output,
            BackupError,
            CommonBackupError,
            ForLogOnly
        };
        public static void Report(String text, TextType t)
        {
            switch (t)
            {
                case TextType.InitialError:
                    MessageBox.Show(text);
                    break;
                default:
                    SendReportToUI(text, t);
                    break;
            }
        }

        private static void SendReportToUI(string text, TextType theType)
        {
            OnReportText(new TextEvent(text, theType));
        }

        public static event EventHandler ReportText;
        public static void OnReportText(TextEvent e)
        {
            if (ReportText != null)
            {
                ReportText(null, e);
            }
        }

        public static void ReportProgressToUI(long currentProgress, long currentMax, long overallProgress, long overallMax)
        {
            if (!BackupRunnerViewModel.Instance.RunAutomatically)
            {
                OnReportProgress(new ProgressEvent(currentProgress, currentMax, overallProgress, overallMax));
            }
        }

        public static event EventHandler ReportProgress;
        public static void OnReportProgress(ProgressEvent e)
        {
            if (ReportProgress != null)
            {
                ReportProgress(null, e);
            }
        }

        public static void ReportCommonCopyError(string longMessage, string source, string destination, Exception excep)
        {
            OverwriteErrorEvent tr = new OverwriteErrorEvent(longMessage, TextType.CommonBackupError, source, destination, excep);
            OnReportText(tr);
        }

        public static void ReportDeleteError(string longMessage, string file, Exception e)
        {
            DeleteErrorEvent de = new DeleteErrorEvent(longMessage, TextType.CommonBackupError, file, e);

        }
    }
}
