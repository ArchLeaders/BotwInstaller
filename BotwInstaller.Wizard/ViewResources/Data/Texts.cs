using BotwInstaller.Lib;
using BotwInstaller.Wizard.Helpers;
using BotwInstaller.Wizard.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace BotwInstaller.Wizard.ViewResources.Data
{
    public static class Texts
    {
        public static Dictionary<string, HandledException> HandledExceptions { get; set; } = new()
        {
            {
                "Network Error|The request was canceled due to the configured HttpClient.Timeout of ", new()
                {
                    Exception = "The download request canceled because the configured timeout elapsed.",
                    Message = "The server took too long to respond.",
                    ExtendedMessage = "Please confirm you are connected to the internet.\n" +
                        "If you are but the problem still opccurs, go back to the splash screen and increase the **timeout** value in the bottom right corner and try again."
                }
            },
        };

        public static string PiracyWarning { get => new(
                "To legally obtain The Legend of Zelda: Breath of the Wild you must dump " +
                "it from your WiiU. Alternatively you can dump your WiiU online files and download " +
                "the game legally from Nintendo's server through Cemu.\n\n" +
                "[WiiU Homebrew Guide](https://wiiu.hacks.guide/#/) \n" +
                "[Dumping Video](https://www.youtube.com/watch?v=bFTgv5mzSg8&t=300s) \n" +
                "[Discord Help Server](https://discord.gg/cbA3AWwfJj)\n"
            );
        }
    }
}
