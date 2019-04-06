using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using System.Management;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OliinykLab5.Models
{
    internal class ProcessInfo: INotifyPropertyChanged
    {
        private PerformanceCounter _cpuv;
        private PerformanceCounter _ramv;
        private readonly Process _process;
        private readonly string _name;
        private readonly int _id;
        private bool _isActive;
        private double _cpu;
        private double _ram;
        private double _ramV;
        private int _threadsQuontity;
        private readonly string _userName;
        private readonly string _fileName;
        private readonly string _filePath;
        private readonly string _startTime;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public ProcessInfo(Process process)
        {
                _process = process;
                _name = process.ProcessName;
                _id = process.Id;
                _userName = GetUserName();
                
                _isActive = IsActive = Process.GetProcesses().Where(p => p.Id == Id).Count() > 0;
                _threadsQuontity = process.Threads.Count;
                CalculateRAMAndCPU();

            try
            {
                _startTime = process.StartTime.ToString();
                _filePath = _process.MainModule.FileName;
                FileInfo fi = new FileInfo(_filePath);
                _fileName = fi.Name;
            }
            catch (Exception)
            {
                _filePath = "No access";
                _fileName = "No access";
                _startTime = "No access";
            }

        }
        

        private string GetUserName()
        {
            try
            {
                var sq = new ObjectQuery("Select * from Win32_Process Where ProcessID = '" + Id + "'");
                var searcher = new ManagementObjectSearcher(sq);
                var process = searcher.Get().Cast<ManagementObject>().First();
                var ownerInfo = new string[2];
                process.InvokeMethod("GetOwner", ownerInfo);
                string result = ownerInfo[0];
                if (String.IsNullOrEmpty(result)) return "No information";
                else return result;
            }
            catch (InvalidOperationException)
            {
                return "No information";
            }
           
        }

        internal void Update()
        {
            CalculateRAMAndCPU();
            IsActive = Process.GetProcesses().Where(p=>p.Id==Id).Count()>0;
            ThreadsQuontity = _process.Threads.Count;
        }

        private void CalculateRAMAndCPU()
        {
            if (_cpuv==null) _cpuv = new PerformanceCounter("Process", "% Processor Time", Name, true);
            if (_ramv == null) _ramv = new PerformanceCounter("Process", "Private Bytes", Name, true);

            try
            {
                CPU = Math.Round((_cpuv.NextValue() / Environment.ProcessorCount), 1);
            }
            catch (InvalidOperationException)
            {
                CPU = 0;
            }
            
             try
             {
                RAMV = Math.Round((_ramv.NextValue() / 1048576), 1);
                Computer myComputer = new Computer();
                double allRAM = myComputer.Info.TotalVirtualMemory;
                RAM = Math.Round((_ramv.NextValue()* 100/allRAM), 1);
            
             }
            catch (InvalidOperationException)
            {
                RAMV = 0;
                RAM = 0;
            }

        }

        internal ObservableCollection<ProcessModule> Modules()
        {
            
                ObservableCollection<ProcessModule> result = new ObservableCollection<ProcessModule>();
                try
                {
                    foreach (ProcessModule modul in _process.Modules)
                    {
                        result.Add(modul);
                    }
                 }
                catch (Exception)
                {
                }

            return result;
        }

        internal ObservableCollection<ProcessThread> Threads()
        {

            ObservableCollection<ProcessThread> result = new ObservableCollection<ProcessThread>();
            try
            {
                foreach (ProcessThread thread in _process.Threads)
                {
                    result.Add(thread);
                }
            }
            catch (Exception)
            {
            }
            

            return result;
        }

        public string Name
        {
            get { return _name; }
        }

        public int Id
        {
            get { return _id; }
        }

        public bool IsActive
        {
            get { return _isActive; }
            private set
            {
                _isActive = value; 
                OnPropertyChanged("IsActive");
            }
        }

        public double CPU
        {
            get { return _cpu; }
            private set
            {
                _cpu = value;
                OnPropertyChanged("CPU");
            }
        }

        public double RAM
        {
            get { return _ram; }
            private set
            {
                _ram = value;
                OnPropertyChanged("RAM");
            }
        }

        public double RAMV
        {
            get { return _ramV; }
            private set
            {
                _ramV = value;
                OnPropertyChanged("RAMV");
            }
        }

        public int ThreadsQuontity
        {
            get { return _threadsQuontity; }
            private set
            {
                _threadsQuontity = value;
                OnPropertyChanged("ThreadsQuontity");
            }
        }

        public string UserName
        {
            get { return _userName; }
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public string FilePath
        {
            get { return _filePath; }
        }

        public string StartTime
        {
            get { return _startTime; }
        }

        public Process Process
        {
            get { return _process; }
        }
       
    }
}
