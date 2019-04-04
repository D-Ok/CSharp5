using OliinykLab5.ViewModels;
using System.Windows.Controls;

namespace OliinykLab5.Views
{
    /// <summary>
    /// Логика взаимодействия для AllProcessesView.xaml
    /// </summary>
    public partial class AllProcessesView : UserControl
    {
        public AllProcessesView()
        {
            InitializeComponent();
            DataContext = new AllProcessesViewModel();
        }
    }
}
