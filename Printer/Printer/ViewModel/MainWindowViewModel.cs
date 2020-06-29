using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Printer.ViewModel
{
    class MainWindowViewModel:ViewModelBase
    {
        #region Fields
        private MainWindow main;
        BackgroundWorker backgroundWorker = new BackgroundWorker();
        private string file = " ";
        private static bool _isRunning=false;
        #endregion

        /// <summary>
        /// Main Window VM constructor with one parameter
        /// </summary>
        /// <param name="mainOpen"></param>
        public MainWindowViewModel(MainWindow mainOpen)
        {
            main = mainOpen;
            backgroundWorker.DoWork += PrintDoWork;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.ProgressChanged += PrintProgressChanged;
            backgroundWorker.RunWorkerCompleted += PrintRunWorkerCompleted;
        }

        #region Properties
        /// <summary>
        /// Text printed into file
        /// </summary>
        private string txtFile;
        public string TxtFile
        {
            get { return txtFile; }
            set
            {
                txtFile = value;
                OnPropertyChanged("TxtFile");
            }
        }

        /// <summary>
        /// File copies property
        /// </summary>
        private string fileCopy;
        public string FileCopy
        {
            get { return fileCopy; }
            set
            {
                fileCopy = value;
                OnPropertyChanged("FileCopy");
            }
        }

        /// <summary>
        /// Current progress bar property
        /// </summary>
        private int currentProgress;
        public int CurrentProgress
        {
            get { return currentProgress; }
            private set
            {
                if (currentProgress != value)
                {
                    currentProgress = value;
                    OnPropertyChanged("CurrentProgress");
                }
            }
        }

        /// <summary>
        /// Property for print progress finished percentage
        /// </summary>
        private string printFinishedInfo;
        public string PrintFinishedInfo
        {
            get { return printFinishedInfo; }
            set
            {
                printFinishedInfo = value;
                OnPropertyChanged("PrintProgress");
            }
        }

        /// <summary>
        /// Warning info message property
        /// </summary>
        private string warningInfo;
        public string WarningInfo
        {
            get { return warningInfo; }
            set
            {
                warningInfo = value;
                OnPropertyChanged("WarningInfo");
            }
        }
        #endregion


        private void PrintDoWork(object sender, DoWorkEventArgs e)
        {

            for(int i=1; i<int.Parse(fileCopy)+1;i++)
            {
                Thread.Sleep(1000);

                //if cancellation is requested, progress percentage resets to 0
                if(backgroundWorker.CancellationPending)
                {
                    e.Cancel = true;
                    backgroundWorker.ReportProgress(0);
                    return;
                }
                //naming file copies
                file = i + "." + DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year + "_" + DateTime.Now.Hour + "_" +
                    DateTime.Now.Minute;

                //writing text into file
                using (StreamWriter sw = new StreamWriter(file))
                {
                    sw.WriteLine(txtFile);
                }
                //
                if(i==int.Parse(fileCopy))
                {
                    //progress bar is 100% if all copies are printed
                    backgroundWorker.ReportProgress(100);
                }
                else
                {
                    //progress bar state depending of no of printed file copies
                    backgroundWorker.ReportProgress(100 / int.Parse(fileCopy) * i);
                }
            }
            WarningInfo = " ";
        }

        private void PrintProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            CurrentProgress = e.ProgressPercentage;

            //if progress is finished, print message that printing is finished
            if(CurrentProgress==100)
            {
                PrintFinishedInfo = "Printing is finished.";
            }
            else
            {
                //print percentage of finished job
                PrintFinishedInfo = currentProgress.ToString() + "%";
            }
        }

        private void PrintRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Cancelled)
            {
                PrintFinishedInfo = "Printing is cancelled.";
                WarningInfo = " ";
                _isRunning = false;
            }
            else if(e.Error !=null)
            {
                printFinishedInfo = e.Error.Message;
            }
        }
    }
}
