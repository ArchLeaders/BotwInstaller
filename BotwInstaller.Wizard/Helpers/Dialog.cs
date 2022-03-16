#pragma warning disable CS8629

using BotwInstaller.Wizard.ViewModels;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotwInstaller.Wizard.Helpers
{
    public static class Dialog
    {
        public static bool Show(this IWindowManager winManager, string message, string title = "Notice", bool isYesNo = false, string? exColor = null, double width = 220)
        {
            MessageViewModel promptViewModel = new(message, title, isYesNo, exColor, width);
            return !(bool)winManager.ShowDialog(promptViewModel);
        }

        public static bool Error(this IWindowManager winManager, string message, string? exMessage = null, string title = "Unhandled Exception", bool isYesNo = false, string? exColor = null)
        {
            HandledErrorViewModel promptViewModel = new(message, title, isYesNo, exMessage, exColor);
            return !(bool)winManager.ShowDialog(promptViewModel);
        }
    }
}
