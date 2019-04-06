using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Runtime.CompilerServices;
using OliinykLab5.Models;
using System.Collections.Generic;
using OliinykLab5.Tools.Managers;
using System.Linq;
using System.Windows.Input;
using System;
using OliinykLab5.Tools;
using System.IO;
using System.Windows;

namespace OliinykLab5.ViewModels
{
    class AllProcessesViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ProcessInfo> _processes;
        private ProcessInfo _selectedProcess;
        private string _currentProcess;

        private ObservableCollection<ProcessModule> _selectedProcessModules;
        private ObservableCollection<ProcessThread> _selectedProcessThreads;

        private Thread _workingThreadRefreshList;
        private Thread _workingThreadUpdating;
        private CancellationToken _token;
        private CancellationTokenSource _tokenSource;
        
        private ICommand _openFolderCommand;
        private ICommand _killProcessCommand;

        private bool _isDescending;
        private string _selectedProperty;
        private readonly string[] _propertiesList =
            {"Id", "Name", "Is active", "CPU", "RAM", "Threads", "User name", "Sourse name", "Sourse", "Start time"};


        public string[] PropertiesList
        {
            get { return _propertiesList; }
        }

        public bool IsDescending
        {
            get { return _isDescending; }
            set
            {
                _isDescending = value;
                if (_selectedProperty != null) SortAndRefreshList(null);
                OnPropertyChanged("IsDescending");
            }
        }

        public string SelectedProperty
        {
            get { return _selectedProperty; }
            set
            {
                _selectedProperty = value;
                SortAndRefreshList(null);
                OnPropertyChanged("SelectedProperty");
            }
        }

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
                if(value != null)
                {
                    CurrentProcess = $"Selected Process: Name={_selectedProcess.Name}, Id={_selectedProcess.Id}";
                    SelectedProcessThreads = _selectedProcess.Threads();
                    SelectedProcessModules = _selectedProcess.Modules();
                }
                else
                {
                    CurrentProcess = "";
                    SelectedProcessThreads = null;
                    SelectedProcessModules = null;
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
            _workingThreadRefreshList = new Thread(WorkingThreadRefreshListProcess);
            _workingThreadUpdating = new Thread(WorkingThreadUpdating);
            _workingThreadRefreshList.Start();
            _workingThreadUpdating.Start();
        }

        private void WorkingThreadRefreshListProcess()
        {
            int i = 0;
            while (!_token.IsCancellationRequested)
            {
                List<ProcessInfo> curProcessInfoRefreshed = _processes.ToList();
                
                LoaderManager.Instance.ShowLoader();
                
                List<int> curProcessesId = new List<int>();
                foreach (var process in _processes)
                {
                    curProcessesId.Add(process.Id);
                }

                List<int> processesId = new List<int>();
                foreach (var process in Process.GetProcesses())
                {
                    if(!curProcessesId.Contains(process.Id)) curProcessInfoRefreshed.Add(new ProcessInfo(process));
                    processesId.Add(process.Id);
                }

                IEnumerable<ProcessInfo> toRemove ;
                foreach (var id in curProcessesId)
                {
                    if (!processesId.Contains(id))
                    {
                        toRemove = curProcessInfoRefreshed.Where(p => p.Id == id);
                        curProcessInfoRefreshed.Remove(toRemove.ElementAt(0));
                    }
                }
                
                SortAndRefreshList(curProcessInfoRefreshed);

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

        private void WorkingThreadUpdating()
        {
            int i = 0;
            while (!_token.IsCancellationRequested)
            {
                if (_selectedProcess != null)
                {
                    SelectedProcessThreads = _selectedProcess.Threads();
                    SelectedProcessModules = _selectedProcess.Modules();
                }
                SortAndRefreshList(null);
                foreach (var process in Processes)
                {
                    process.Update();
                }
                SortAndRefreshList(null);

                if (_token.IsCancellationRequested)
                    break;
               
                for (int j = 0; j < 3; j++)
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
            _workingThreadRefreshList.Join(2000);
            _workingThreadRefreshList.Abort();
            _workingThreadRefreshList = null;

            _workingThreadUpdating.Join(2000);
            _workingThreadUpdating.Abort();
            _workingThreadUpdating = null;
        }

       

        private bool CanSelectedProcessCommandsExecute(object obj)
        {
            return _selectedProcess!=null;
        }

    

        public ICommand OpenFolderCommand
        {
            get
            {
                return _openFolderCommand ?? (_openFolderCommand = new RelayCommand<object>(OpenFolderImplementation, CanSelectedProcessCommandsExecute));
            }
        }

        public ICommand KillProcessCommand
        {
            get
            {
                return _killProcessCommand ?? (_killProcessCommand = new RelayCommand<object>(KillProcessImplementation, CanSelectedProcessCommandsExecute));
            }
        }

        private void SortAndRefreshList(List<ProcessInfo> list)
        {
                IEnumerable<ProcessInfo> sorted = list;
                if (sorted == null) sorted = Processes.ToList();

                switch (SelectedProperty)
                {
                    case "Id":
                        if (IsDescending) sorted = sorted.OrderByDescending(p => p.Id);
                        else sorted = sorted.OrderBy(p => p.Id);
                        break;
                    case "Name":
                        if (IsDescending) sorted = sorted.OrderByDescending(p => p.Name);
                        else sorted = sorted.OrderBy(p => p.Name);
                        break;
                    case "Is active":
                        if (IsDescending) sorted = sorted.OrderByDescending(p => p.IsActive);
                        else sorted = sorted.OrderBy(p => p.IsActive);
                        break;
                    case "CPU":
                        if (IsDescending) sorted = sorted.OrderByDescending(p => p.CPU);
                        else sorted = sorted.OrderBy(p => p.CPU);
                        break;
                    case "RAM":
                        if (IsDescending) sorted = sorted.OrderByDescending(p => p.RAM);
                        else sorted = sorted.OrderBy(p => p.RAM);
                        break;
                    case "Threads":
                        if (IsDescending) sorted = sorted.OrderByDescending(p => p.ThreadsQuontity);
                        else sorted = sorted.OrderBy(p => p.ThreadsQuontity);
                        break;
                    case "User name":
                        if (IsDescending) sorted = sorted.OrderByDescending(p => p.UserName);
                        else sorted = sorted.OrderBy(p => p.UserName);
                        break;
                    case "Sourse name":
                        if (IsDescending) sorted = sorted.OrderByDescending(p => p.FileName);
                        else sorted = sorted.OrderBy(p => p.FileName);
                        break;
                    case "Sourse":
                        if (IsDescending) sorted = sorted.OrderByDescending(p => p.FilePath);
                        else sorted = sorted.OrderBy(p => p.FilePath);
                        break;
                    case "Start time":
                        if (IsDescending) sorted = sorted.OrderByDescending(p => p.StartTime);
                        else sorted = sorted.OrderBy(p => p.StartTime);
                        break;
                    default: break;
                }

                ProcessInfo curProcess = SelectedProcess;

                Processes = new ObservableCollection<ProcessInfo>(sorted);
                SelectedProcess = (Processes.Contains(curProcess)) ? curProcess : null;
        }

        
        private void OpenFolderImplementation(object obj)
        {
            string path = _selectedProcess.FilePath;
            if (String.IsNullOrEmpty(path)) MessageBox.Show("No access to path of this file");
            if (File.Exists(path))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $" /select, {path}"));
            }
        }

        private void KillProcessImplementation(object obj)
        {
          Process.GetProcessById(_selectedProcess.Id).Kill();
          SelectedProcess = null;
        }

    }
}
