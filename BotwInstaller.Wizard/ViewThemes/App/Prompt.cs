using BotwInstaller.Wizard.Views;
using Stylet;

namespace BotwInstaller.Wizard.ViewThemes.App
{
    public static class Prompt
    {
        public static bool Show(string message, string title = "Notice", bool isYesNo = false)
        {
            // PromtView prompt = new(title, message, isYesNo);
            // prompt.ShowDialog();

            // return prompt.activePrompt;
            return true;
        }
    }
}
