#pragma warning disable IDE0060

using System.Operations;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace BotwInstaller.Wizard.Helpers
{
    class Actions
    {
        public static void OpenURL(Hyperlink sender, RequestNavigateEventArgs e) => Execute.Url(sender.NavigateUri);
    }
}
