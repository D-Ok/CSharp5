using System;
using System.Windows;
using OliinykLab5.Models;

namespace OliinykLab5.Tools.Managers
{
    internal static class StationManager
    {
        public static event Action StopThreads;

        internal static ProcessInfo CurrentProcess { get; set; }
    
        internal static void CloseApp()
        {
            MessageBox.Show("ShutDown");
            StopThreads?.Invoke();
            Environment.Exit(1);
        }
    }
}
