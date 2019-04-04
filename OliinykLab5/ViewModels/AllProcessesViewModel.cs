using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Runtime.CompilerServices;
using OliinykLab5.Models;
using System.Collections.Generic;
using OliinykLab5.Tools.Managers;
using System.Linq;

namespace OliinykLab5.ViewModels
{
    class AllProcessesViewModel : INotifyPropertyChanged
    {

        private ObservableCollection<ProcessInfo> _processes;
        private Thread _workingThread;
        private Thread _workingThread1;

        private ProcessInfo _selectedProcess;

        private string _currentProcess;

        private ObservableCollection<ProcessModule> _selectedProcessModules;

        private ObservableCollection<ProcessThread> _selectedProcessThreads;

        private CancellationToken _token;
        private CancellationTokenSource _tokenSource;
        // private CancellationToken _token;
        // private CancellationTokenSource _tokenSource;

        public ObservableCollection<ProcessInfo> Processes
        {
            get { return _processes; }
            private set
            {
                _processes = value;
                OnPropertyChanged();
            }
        }

        public string CurrentProcess
        {
            get { return _currentProcess; }
            set
            {
                _currentProcess = value;
                OnPropertyChanged();
            }
        }

        public ProcessInfo SelectedProcess
        {
            get { return _selectedProcess; }
            set
            {
                _selectedProcess = value;
                StationManager.CurrentProcess = value;
                if (value != null)
                {
                    CurrentProcess = $"Selected Person: Name={_selectedProcess.Name}, Id={_selectedProcess.Id}";
                    SelectedProcessThreads = _selectedProcess.Threads();
                    SelectedProcessModules = _selectedProcess.Modules();
                }
                OnPropertyChanged();
                
            }
        }

        public ObservableCollection<ProcessThread> SelectedProcessThreads
        {
            get { return _selectedProcessThreads; }
            private set
            {
                _selectedProcessThreads = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ProcessModule> SelectedProcessModules
        {
            get { return _selectedProcessModules; }
            private set
            {
                _selectedProcessModules = value;
                OnPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        internal AllProcessesViewModel()
        {
            _processes = new ObservableCollection<ProcessInfo>();

            foreach (var process in Process.GetProcesses())
            {
                _processes.Add(new ProcessInfo(process));
            }

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            StartWorkingThread();
            StationManager.StopThreads += StopWorkingThread;
        }

        private void StartWorkingThread()
        {
            _workingThread = new Thread(WorkingThreadProcess);
            _workingThread1 = new Thread(WorkingThreadProcess1);
            _workingThread.Start();
            _workingThread1.Start();
        }

        private void WorkingThreadProcess()
        {
            int i = 0;
            while (!_token.IsCancellationRequested)
            {
                List<ProcessInfo> curProcesses = new List<ProcessInfo>();
               
                foreach (var process in Process.GetProcesses())
                {
                    curProcesses.Add(new ProcessInfo(process));
                }
                LoaderManager.Instance.ShowLoader();
                Processes = new ObservableCollection<ProcessInfo>(curProcesses);

                for (int j = 0; j < 3; j++)
                {
                    Thread.Sleep(500);
                    if (_token.IsCancellationRequested)
                        break;
                }
                if (_token.IsCancellationRequested)
                    break;
                LoaderManager.Instance.HideLoader();
                for (int j = 0; j < 10; j++)
                {
                    Thread.Sleep(500);
                    if (_token.IsCancellationRequested)
                        break;
                }
                if (_token.IsCancellationRequested)
                    break;
                i++;
            }
        }

        private void WorkingThreadProcess1()
        {
            int i = 0;
            while (!_token.IsCancellationRequested)
            {
                
                foreach (var process in Processes)
                {
                    process.CalculateRAMAndCPU();
                }
                //LoaderManager.Instance.ShowLoader();

                for (int j = 0; j < 3; j++)
                {
                    Thread.Sleep(500);
                    if (_token.IsCancellationRequested)
                        break;
                }
                if (_token.IsCancellationRequested)
                    break;
               // LoaderManager.Instance.HideLoader();
                for (int j = 0; j < 4; j++)
                {
                    Thread.Sleep(500);
                    if (_token.IsCancellationRequested)
                        break;
                }
                if (_token.IsCancellationRequested)
                    break;
                i++;
            }
        }

        internal void StopWorkingThread()
        {
            _tokenSource.Cancel();
            _workingThread.Join(2000);
            _workingThread.Abort();
            _workingThread = null;

            _workingThread1.Join(2000);
            _workingThread1.Abort();
            _workingThread1 = null;
        }
    }
}
