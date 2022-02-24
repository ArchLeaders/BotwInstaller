using BotwInstaller.Wizard.ViewModels;
using System;
using System.Windows;
using System.Windows.Threading;

namespace BotwInstaller.Wizard.Views
{
    /// <summary>
    /// Interaction logic for PromptView.xaml
    /// </summary>
    public partial class PromptView : Window
    {
        public PromptView()
        {
            InitializeComponent();
            DataContext = new PromptViewModel("No details were provided.");
            SourceInitialized += (s, a) =>
            {
                Dispatcher.Invoke(InvalidateVisual, DispatcherPriority.Input);
            };

            btnOk.Focus();
        }

        private void Window_OnContentRendered(object sender, EventArgs e)
        {
            InvalidateVisual();
        }
    }
}
