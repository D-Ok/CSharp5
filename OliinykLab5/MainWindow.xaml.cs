using OliinykLab5.Tools.Managers;
using OliinykLab5.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;


namespace OliinykLab5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            InitializeApplication();
        }

        private void InitializeApplication()
        {
           // StationManager.Initialize();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            StationManager.CloseApp();
        }
    }
}