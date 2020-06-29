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
            //add method to DoWork event
            backgroundWorker.DoWork += PrintDoWork;
            
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            //add method to ProgressChanged event
            backgroundWorker.ProgressChanged += PrintProgressChanged;
            //add method to RunWorkerCompleted event
            backgroundWorker.RunWorkerCompleted += PrintRunWorkerCompleted;
        }

        #region Properties
        /// <summary>
        /// Text input that will be printed into file
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
        /// File copies property defining how many file copies user inputed
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
        /// Property for showing messages about print progress 
        /// </summary>
        private string messageInfo;
        public string MessageInfo
        {
            get { return messageInfo; }
            set
            {
                messageInfo = value;
                OnPropertyChanged("MessageInfo");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Method for text printing in document
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintDoWork(object sender, DoWorkEventArgs e)
        {
            //parsing input for number of file copies
            int number = int.Parse(FileCopy);
            int percentResult = 100 / number;
            //files creation and writing text in it
            for (int i = 1; i <= number; i++)
            {
                //printing lasts for 1000ms
                Thread.Sleep(1000);

                //if cancellation is requested, progress percentage resets to 0
                if (backgroundWorker.CancellationPending)
                {
                    e.Cancel = true;
                    backgroundWorker.ReportProgress(0);
                    return;
                }
                //naming file copies
                file = string.Format(@"../../{0}.{1}_{2}_{3}_{4}_{5}.txt", i, DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year,
                                    DateTime.Now.Hour, DateTime.Now.Minute);

                //writing text into file
                using (StreamWriter sw = new StreamWriter(file))
                {
                    sw.WriteLine(txtFile);
                }
                //invoke method and raise ProgressChanged event
                backgroundWorker.ReportProgress(percentResult * i);                
            }
        }

        /// <summary>
        /// Update label on user interface with message about print progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            CurrentProgress = e.ProgressPercentage;
            MessageInfo = e.ProgressPercentage.ToString() + "%";
            
        }

        /// <summary>
        /// Method for showing state of file printing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Cancelled)
            {
                MessageInfo = "Printing is cancelled.";                
            }
            //if error occured during printing
            else if(e.Error !=null)
            {
                MessageInfo = e.Error.Message.ToString();
            }
            else
            {
                //if progress is finished, print message that printing is finished
                MessageInfo = "Printing is finished.";
            }
        }
        #endregion

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
                MessageInfo = "Printing in progress... Please wait.";
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
