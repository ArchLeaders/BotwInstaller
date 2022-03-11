using BotwInstaller.Wizard.ViewModels;
using System.Windows.Controls;

namespace BotwInstaller.Wizard.Views
{
    /// <summary>
    /// Interaction logic for SetupView.xaml
    /// </summary>
    public partial class SetupView : UserControl
    {
        public SetupView()
        {
            InitializeComponent();
            DataContext = new SetupViewModel();
        }
    }
}
