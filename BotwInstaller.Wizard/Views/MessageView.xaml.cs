using BotwInstaller.Wizard.ViewModels;
using System.Windows;
using System.Windows.Threading;

namespace BotwInstaller.Wizard.Views
{
    /// <summary>
    /// Interaction logic for MessageView.xaml
    /// </summary>
    public partial class MessageView : Window
    {
        public MessageView()
        {
            InitializeComponent();
            DataContext = new MessageViewModel("No details were provided.");
            SourceInitialized += (s, a) =>
            {
                Dispatcher.Invoke(InvalidateVisual, DispatcherPriority.Input);
            };

            btnOk.Focus();
        }
    }
}
