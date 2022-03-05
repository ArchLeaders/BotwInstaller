#pragma warning disable CS8629

using BotwScripts.Lib.Common.ClassObjects.Json;
using System.Collections.Generic;
using System.Windows.Controls;

namespace BotwInstaller.Wizard.ViewThemes.App
{
    public class ConfigSetter
    {
        public static BotwInstallerConfig? Init(List<Control> data)
        {
            BotwInstallerConfig cc = new();

            foreach (var item in data)
            {
                var trueName = item.Name.Replace("tb_", "").Replace("cb_", "");
                if (item.Name.StartsWith("tb_"))
                {
                    var tb = (TextBox)item;

                    // Base Dir
                    if (nameof(cc.BaseDir) == trueName)
                        cc.BaseDir = tb.Text;
                    if (nameof(cc.BetterjoyDir) == trueName)
                        cc.BetterjoyDir = tb.Text;
                    if (nameof(cc.BaseDir) == trueName)
                        cc.BaseDir = tb.Text;
                    if (nameof(cc.BaseDir) == trueName)
                        cc.BaseDir = tb.Text;
                    if (nameof(cc.BaseDir) == trueName)
                        cc.BaseDir = tb.Text;
                    if (nameof(cc.BaseDir) == trueName)
                        cc.BaseDir = tb.Text;
                    if (nameof(cc.BaseDir) == trueName)
                        cc.BaseDir = tb.Text;
                    if (nameof(cc.BaseDir) == trueName)
                        cc.BaseDir = tb.Text;
                    if (nameof(cc.BaseDir) == trueName)
                        cc.BaseDir = tb.Text;
                    if (nameof(cc.BaseDir) == trueName)
                        cc.BaseDir = tb.Text;
                    if (nameof(cc.BaseDir) == trueName)
                        cc.BaseDir = tb.Text;
                }
                else if (item.Name.StartsWith("cb_"))
                {
                    var cb = (CheckBox)item;

                    if (nameof(cc.Install.BetterJoy) == trueName)
                        cc.Install.BetterJoy = (bool)cb.IsChecked;
                }
                else
                {
                    PromptActions.Show("(Control)data.Key did not have any identifier.\n\nBotwInstaller.Wizard.ViewThemes.App.ConfigSetter.Init(Dict<Control, object)", "Error");
                    return null;
                }
            }

            return cc;
        }
    }
}
