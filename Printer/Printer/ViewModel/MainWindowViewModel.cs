using Printer.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

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

        #region Commands
        /// <summary>
        /// Print command
        /// </summary>
        private ICommand print;
        public ICommand Print
        {
            get
            {
                if(print==null)
                {
                    print = new RelayCommand(param => PrintExecute(), param => CanPrintExecute());
                }
                return print;
            }
        }

        /// <summary>
        /// Print execution method
        /// </summary>
        private void PrintExecute()
        {
            //if background worker is not busy it will start asynchron execution
            if(!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();
                _isRunning = true;
            }
            else
            {
                //if its busy it will show message that printing is in progress
                WarningInfo = "Printing in progress... Please wait.";
            }
        }

        /// <summary>
        /// Method for confirming if printing is possible or not
        /// </summary>
        /// <returns>true or false</returns>
        private bool CanPrintExecute()
        {
            //if there is no text or copies of files print is not possible
            if (TxtFile == null || FileCopy == null || int.Parse(FileCopy) == 0)
            {
                return false;
            }
            else
                return true;
        }

        /// <summary>
        /// Cancel command
        /// </summary>
        private ICommand cancel;
        public ICommand Cancel
        {
            get
            {
                if (cancel == null)
                {
                    cancel = new RelayCommand(param => CancelExecute(), param => CanCancelExecute());
                }
                return cancel;
            }
        }

        /// <summary>
        /// Method for canceling printing 
        /// </summary>
        private void CancelExecute()
        {
            //if background worker is busy it possible to cancel print job
            if(backgroundWorker.IsBusy)
            {
                backgroundWorker.CancelAsync();
                _isRunning = false;
            }           

        }

        /// <summary>
        /// Method for decision if printing cancellation is possible or not
        /// </summary>
        /// <returns>true or false</returns>
        private bool CanCancelExecute()
        {
            //if printing is in progress cancel is possible
            if(_isRunning==true)
            {
                return true;
            }
            else
                return false;
        }
        #endregion
    }
}
