using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Devices;
using System.Management;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Runtime.CompilerServices;

namespace OliinykLab5.Models
{
    internal class ProcessInfo: INotifyPropertyChanged
    {
        private Process _process;
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
        private readonly DateTime _startTime;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public ProcessInfo(Process process)
        {
            try
            {
                _process = process;
                _name = process.ProcessName;
                _id = process.Id;
                _isActive = process.Responding;
                _threadsQuontity = process.Threads.Count;

                _userName = GetUserName();
               _filePath = _process.MainModule.FileName;
              //MessageBox.Show(FileName);
               //_fileName = Name + ".exe";
                FileInfo fi = new FileInfo(_filePath);
                _fileName = fi.Name;
                _startTime = process.StartTime;
                CalculateRAMAndCPU();
                
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                //_startTime = new DateTime(2020, 12, 12);
            }
            catch (Exception e)
            {
            //    _startTime= new DateTime(2020, 12, 12);
            }

        }

        private string GetUserName()
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

        internal void CalculateRAMAndCPU()
        {
            PerformanceCounter cpu = new PerformanceCounter("Process", "% Processor Time", Name, true);
            PerformanceCounter ramv = new PerformanceCounter("Process", "Private Bytes", Name, true);
            //PerformanceCounter ram = new PerformanceCounter("Process", "% Private Bytes", Name, true);

            //double processCPU = Convert.ToDouble(cpu.NextValue());
            try
            {
                CPU = Math.Round((cpu.NextValue() / Environment.ProcessorCount), 3);
            }
            catch (System.InvalidOperationException e)
            {
                CPU = 0;
            }
            
             //CPU = Math.Round(cpu.NextValue(), 3);
            RAMV = Math.Round((ramv.NextValue()/1048576), 3);

            Computer myComputer = new Computer();
            double allRAM = myComputer.Info.TotalVirtualMemory;
            RAM = Math.Round((ramv.NextValue()* 100/allRAM), 3);
            IsActive = _process.Responding;

            cpu.Dispose();
            ramv.Dispose();
        }

        internal ObservableCollection<ProcessModule> Modules()
        {
            
                ObservableCollection<ProcessModule> result = new ObservableCollection<ProcessModule>();
                try
                {
                    foreach (System.Diagnostics.ProcessModule modul in _process.Modules)
                    {
                        result.Add(modul);
                    }
                 }
                catch (Exception e)
                {
                }
                
           
            return result;
        }

        internal ObservableCollection<ProcessThread> Threads()
        {

            ObservableCollection<ProcessThread> result = new ObservableCollection<ProcessThread>();
            try
            {
                foreach (System.Diagnostics.ProcessThread thread in _process.Threads)
                {
                    result.Add(thread);
                }
            }
            catch (Exception e)
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
                OnPropertyChanged();
            }
        }

        public double CPU
        {
            get { return _cpu; }
            private set
            {
                _cpu = value;
                OnPropertyChanged();
            }
        }

        public double RAM
        {
            get { return _ram; }
            private set
            {
                _ram = value;
                OnPropertyChanged();
            }
        }

        public double RAMV
        {
            get { return _ramV; }
            private set
            {
                _ramV = value;
                OnPropertyChanged();
            }
        }

        public int ThreadsQuontity
        {
            get { return _threadsQuontity; }
            private set
            {
                _threadsQuontity = value;
                OnPropertyChanged();
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

        public DateTime StartTime
        {
            get { return _startTime; }
        }

       
    }
}
