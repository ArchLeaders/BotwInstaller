using BotwInstaller.Wizard.ViewModels;
using System;
using System.Windows;
using System.Windows.Threading;

namespace BotwInstaller.Wizard.Views
{
    /// <summary>
    /// Interaction logic for PromptView.xaml
    /// </summary>
    public partial class HandledErrorView : Window
    {
        public HandledErrorView()
        {
            InitializeComponent();
            DataContext = new HandledErrorViewModel("No details were provided.");
            SourceInitialized += (s, a) =>
            {
                Dispatcher.Invoke(InvalidateVisual, DispatcherPriority.Input);
            };

            btnOk.Focus();
        }
    }
}
