using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SimpleBackupConsole;
using System.Windows.Documents;
using System.Windows.Media;

namespace BackupUI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = BackupRunnerViewModel.Instance;
            Loaded += ReadConfig;
            BackupRunnerViewModel.Instance.BackupPatternDescriptionChanged += BackupPattenDescriptionChanged;
            BackupRunnerViewModel.Instance.ShutdownRequested += ShutdownRequested;
            BackupRunnerViewModel.Instance.BackupCompleted += BackupCompleted;
            TextReporter.ReportText += TextReported;
            TextReporter.ReportProgress += ProgressReported;
            this._richTextParagraph = new Paragraph();
            BackupPatten_RichTextBox.Document = new FlowDocument(_richTextParagraph);
        }

        private void ProgressReported(object sender, EventArgs e)
        {
            var text = e as ProgressEvent;
            ReportProgressToUI(text.CurrentProgress, text.CurrentMax, text.OverallProgress, text.OverallMax);
        }

        private void ReportProgressToUI(long currentProgress, long currentMax, long overallProgress, long overallMax)
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                    {
                        CurrentDirectoryFilesCopied_ProgressBar.Value= currentProgress;
                        CurrentDirectoryFilesCopied_ProgressBar.Maximum = currentMax;
                        AllFilesCopied_ProgressBar.Maximum = overallMax;
                        AllFilesCopied_ProgressBar.Value = overallProgress;
                    });
        }


        StringBuilder _activityLog = new StringBuilder();
        private void TextReported(object sender, EventArgs e)
        {
            var text = e as TextEvent;
            switch (text.ReportType)
            {
                case TextReporter.TextType.ForLogOnly:
                    _activityLog.AppendLine(text.Text);
                    break;
                default:
                    ReportTextToUI(text.Text, text.ReportType);
                    break;
            }
        }

        private Paragraph _richTextParagraph;
        private void ReportTextToUI(string newText, TextReporter.TextType theType)
        {
            try
            {
                var displayBrush = Brushes.Black;
                switch(theType)
                {
                    case TextReporter.TextType.BackupError:
                        displayBrush = Brushes.Red;
                        break;
                    case TextReporter.TextType.Output:
                        displayBrush = Brushes.Black;
                        break;

                }
                Application.Current.Dispatcher.Invoke(
                    () => 
                    {
                        _richTextParagraph.Inlines.Add(new Run(newText+"\n")
                        {
                            Foreground = displayBrush,
                        });
                    });
            }
            catch (Exception e)
            {
                //May occur on closing when responding to an event, ignore.
            }
        }

        private void BackupCompleted(object sender, EventArgs e)
        {
            ReportTextToUI("Completed.", TextReporter.TextType.Output);
            WriteLog();
            SetUIToFinished();
            if (BackupRunnerViewModel.Instance.RunAutomatically)
            {
                //Only close the window if the backup run wasn't even attempted, due to it being the wrong day.
                if (BackupRunnerViewModel.Instance.CloseUIOnCompleted)
                {
                    Application.Current.Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
                }
            }
        }

        private void SetUIToFinished()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Title = Title + "- Completed";
            });
        }

        private void WriteLog()
        {
            if (!BackupRunnerViewModel.Instance.ShouldWriteLog)
            {
                return;
            }
            string fileName = DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Year;
            String finalName = fileName;
            int curAttempt = 1;
            String dir = @"BackupLogs\";
            while (File.Exists(dir + finalName + ".txt"))
            {
                finalName = fileName + "_" + curAttempt;
                curAttempt++;
            }
            finalName = finalName + ".txt";
            String curText = GetMainText();
            curText = curText + "\n" + _activityLog.ToString();
            File.WriteAllText(dir + finalName, curText);
        }
        private string GetMainText()
        {
            String ans = "";
            Application.Current.Dispatcher.Invoke(() =>
            {
                TextRange textRange = new TextRange(
                BackupPatten_RichTextBox.Document.ContentStart,
                BackupPatten_RichTextBox.Document.ContentEnd
            );
                ans = textRange.Text;
            });
            return ans;
        }

        private void ShutdownRequested(object sender, EventArgs e)
        {
            Process.Start("shutdown", "/s /t 0");
        }

        private void BackupPattenDescriptionChanged(object sender, EventArgs e)
        {
            ClearRichTextBox();
            ReportTextToUI(BackupRunnerViewModel.Instance.FullBackupPatternDescription, TextReporter.TextType.Output);
        }

        private void ClearRichTextBox()
        {
            _richTextParagraph.Inlines.Clear();
        }


        private void ReadConfig(object sender, RoutedEventArgs e)
        {
            ReadBackupPattern();
            if (BackupRunnerViewModel.Instance.RunAutomatically)
            {
                StartBackup();
            }
        }

        private void ReadBackupPattern()
        {
            ConfigViewModel.Instance.ReadConfigData();
            BackupRunnerViewModel.Instance.CurrentBackupPattern = BackupPatternReader.ReadBackup();

        }

        private void StartBackup_Button_Click(object sender, RoutedEventArgs e)
        {
            StartBackup();
        }

        private void StartBackup()
        {
            StartBackup_Button.IsEnabled = false;
            StaggerBackup_CheckBox.IsEnabled = false;
            ReportTextToUI("\n\n-----------------------\nStarting backup.....", TextReporter.TextType.Output);
            var t = new Task(() => BackupRunner.Instance.StartBackup(BackupRunnerViewModel.Instance.CurrentBackupPattern));
            t.Start();
        }
    }
}