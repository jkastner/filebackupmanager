using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using AutomaticBackup;

namespace BackupUI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly StringBuilder _activityLog = new StringBuilder();
        private readonly Paragraph _richTextParagraph;
        private MainViewModel _currentDataContext = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += ReadConfig;
            BackupRunnerViewModel.Instance.BackupPatternDescriptionChanged += BackupPattenDescriptionChanged;
            BackupRunnerViewModel.Instance.BackupCompleted += BackupCompleted;
            TextReporter.ReportText += TextReported;
            TextReporter.ReportProgress += ProgressReported;
            _richTextParagraph = new Paragraph();
            BackupPatten_RichTextBox.Document = new FlowDocument(_richTextParagraph);
            ErrorsListBox.ItemsSource = _currentDataContext.BackupErrors;
            //int x = 200;
            //while (x > 0)
            //{
            //    ReportCommonError(new DeleteErrorEvent("YAAY" + Guid.NewGuid() + Guid.NewGuid() + Guid.NewGuid() + Guid.NewGuid() + Guid.NewGuid() + Guid.NewGuid() + Guid.NewGuid() + Guid.NewGuid() + Guid.NewGuid() + Guid.NewGuid() + Guid.NewGuid() + Guid.NewGuid() + Guid.NewGuid() + Guid.NewGuid() + Guid.NewGuid() + Guid.NewGuid(),
            //    TextReporter.TextType.CommonBackupError, Guid.NewGuid().ToString() + Guid.NewGuid().ToString(), new Exception()));
            //    x--;
            //}
            

        }

        public MainViewModel CurrentDataContext
        {
            get { return _currentDataContext; }
            set { _currentDataContext = value; }
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
                    CurrentDirectoryFilesCopied_ProgressBar.Value = currentProgress;
                    CurrentDirectoryFilesCopied_ProgressBar.Maximum = currentMax;
                    AllFilesCopied_ProgressBar.Maximum = overallMax;
                    AllFilesCopied_ProgressBar.Value = overallProgress;
                });
        }

        private void TextReported(object sender, EventArgs e)
        {
            var text = e as TextEvent;
            switch (text.ReportType)
            {
                case TextReporter.TextType.ForLogOnly:
                    _activityLog.AppendLine(text.Text);
                    break;
                case TextReporter.TextType.CommonBackupError:
                    ReportCommonError(text as ICommonError);
                    break;
                default:
                    ReportTextToUI(text.Text, text.ReportType);
                    break;
            }
        }

        private void ReportCommonError(ICommonError text)
        {
            if (text == null)
            {
                return;
            }
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    _currentDataContext.BackupErrors.Add(text);
                });
        }

        private void ReportTextToUI(String newText, TextReporter.TextType theType)
        {
            try
            {
                SolidColorBrush displayBrush = Brushes.Black;
                switch (theType)
                {
                    case TextReporter.TextType.BackupError:
                        displayBrush = Brushes.Red;
                        break;
                    case TextReporter.TextType.Output:
                        displayBrush = Brushes.Black;
                        break;
                    case TextReporter.TextType.CommonBackupError:
                        displayBrush = Brushes.Blue;
                        break;
                }
                Application.Current.Dispatcher.Invoke(
                    () =>
                    {
                        _richTextParagraph.Inlines.Add(new Run(newText + "\n")
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

        private bool _shutDownCancelRequested = false;
        private void BackupCompleted(object sender, EventArgs e)
        {
            ReportTextToUI("Completed.", TextReporter.TextType.Output);
            WriteLog();
            SetUIToFinished();
            if (ConfigViewModel.Instance.ShutdownComputerOnCompletion)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    DispatcherTimer shutDownWarning = new DispatcherTimer();
                    shutDownWarning.Interval = TimeSpan.FromSeconds(15);
                    shutDownWarning.Tick += ShutDownWarningOnTick;
                    shutDownWarning.Start();
                }));
                
                
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    YesNoTopmost tw = new YesNoTopmost("Shut down confirmation",
                        "Proceed with shut down?\n(Press NO to keep computer on)");
                    tw.ShowActivated = true;
                    tw.ShowDialog();
                    
                    if (!tw.YesWasClicked)
                    {
                        _shutDownCancelRequested = true;
                        ReportTextToUI("\n\nShutdown canceled.", TextReporter.TextType.Output);
                    }
                    if (tw.YesWasClicked)
                    {
                        ShutDownComputer();
                    }
                }));

            }
            //Only close the window if the backup run wasn't even attempted, due to it being the wrong day.
            else if (ConfigViewModel.Instance.CloseWindowOnCompletion || BackupRunnerViewModel.Instance.ShouldClose)
            {
                Application.Current.Dispatcher.Invoke(() => { Close(); });
            }

        }

        private void ShutDownWarningOnTick(object sender, EventArgs eventArgs)
        {
            if (!_shutDownCancelRequested)
            {
                ShutDownComputer();
            }
        }

        private void ShutDownComputer()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Process.Start("shutdown", "/s /t 0");
                Close();
            });
        }

        private void SetUIToFinished()
        {
            Application.Current.Dispatcher.Invoke(() => { Title = Title + "- Completed"; });
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
            String dir = Path.Combine(PathConstants.CurrentDirectory, "BackupLogs");
            if (!Directory.Exists(dir))
            {
                ReportTextToUI("Error - could not write log. Directory "+dir+" did not exist.", TextReporter.TextType.Output);
                return;
            }
            while (File.Exists(Path.Combine(dir, finalName + ".txt")))
            {
                finalName = fileName + "_" + curAttempt;
                curAttempt++;
            }
            finalName = Path.Combine(dir, finalName + ".txt");

            String curText = GetMainText();
            curText = curText + "\n" + _activityLog;
            File.WriteAllText(finalName, curText);
        }

        private string GetMainText()
        {
            String ans = "";
            Application.Current.Dispatcher.Invoke(() =>
            {
                var textRange = new TextRange(
                    BackupPatten_RichTextBox.Document.ContentStart,
                    BackupPatten_RichTextBox.Document.ContentEnd
                    );
                ans = textRange.Text;
            });
            return ans;
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
            CalculateCopyTime_CheckBox.IsEnabled = false;
            ReportTextToUI("\n\n-----------------------\nStarting backup.....", TextReporter.TextType.Output);
            var t =
                new Task(() => BackupRunner.Instance.StartBackup(BackupRunnerViewModel.Instance.CurrentBackupPattern));
            t.Start();
        }

        private void ToggleDays_Clicked(object sender, RoutedEventArgs e)
        {
            ConfigViewModel.Instance.ToggleAllDays();
        }
    }
}