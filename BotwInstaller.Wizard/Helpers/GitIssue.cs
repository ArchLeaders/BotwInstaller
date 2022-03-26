using BotwInstaller.Lib;
using BotwInstaller.Wizard.Helpers;
using BotwInstaller.Wizard.ViewModels;
using BotwInstaller.Wizard.ViewResources;
using BotwInstaller.Wizard.ViewResources.Data;
using BotwScripts.Lib.Common.Computer;
using Microsoft.Win32;
using Octokit;
using Stylet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BotwInstaller.Wizard.Helpers
{
    public static class GitIssue
    {
        private class Item
        {
            public string Key { get; set; } = "";
            public dynamic Value { get; set; } = "";
            public Item(string key, dynamic value)
            {
                Key = key;
                Value = value;
            }
        }

        public static async Task ViewReportAsHtml(this ShellViewModel shell)
        {
            IWindowManager win = shell.WindowManager;
            ExceptionViewModel exView = shell.ExceptionViewModel;

            string htmlFile = $"{Config.AppData}\\Temp\\{new Random().Next(1000, 9999)}-{new Random().Next(1000, 9999)} - {exView.Message} - index.htm";
            string fullReport = FormatReportAsHtml(exView, shell.Conf, shell.InstallViewModel.Log.Replace("\n", "<br>"))
                .Replace(Config.User, "C:\\Users\\admin");

            await File.WriteAllTextAsync(htmlFile, fullReport);
            await HiddenProcess.Start("explorer.exe", $"\"{htmlFile}\"");
        }

        public static async Task ReportAsMarkdown(this ShellViewModel shell)
        {
            // Get shell info
            IWindowManager win = shell.WindowManager;
            ExceptionViewModel exView = shell.ExceptionViewModel;
            string fullReport = FormatReportAsMarkdown(exView, shell.Conf, shell.InstallViewModel.Log);

            // Get repo
            if (!win.Show($"{ToolTips.ReportError}\n\nContinue anyway?", "Privacy Warning", true, width: 500)) return;

            bool isPublic = win.Show("Would you like to upload to the public GitHub repository?\n\nhttps://github.com/ArchLeaders/BotwInstaller", "", true, width: 300);
            string repo = isPublic ? "botwinstaller" : "botwinstaller-issues";

            // Make un-repeatable
            exView.IsReportable = false;

            // Create git client
            var client = new GitHubClient(new ProductHeaderValue("botw-installer-v3"));
            client.Credentials = new Credentials(AuthKey.Get);

            // Get repo issues
            var issues = await client.Issue.GetAllForRepository("archleaders", repo);

            // Update issue if it exists
            foreach (var issue in issues)
            {
                if (issue.Title == "") //shell.Exception
                {
                    IssueUpdate issueUpdate = new();
                    issueUpdate.Body = $"{issue.Body}\n\n---\n\n> {exView.ContactInfo}\n{fullReport.Replace(Config.User, "C:\\Users\\admin")}";

                    await client.Issue.Update("archleaders", repo, issue.Number, issueUpdate);
                    win.Show($"Updated issue: {issue.Id}");
                    return;
                }
            }

            // Create new issue
            var issueNew = await client.Issue.Create("archleaders", repo, new NewIssue(exView.Message)
            {
                Body = $"> {exView.ContactInfo}\n{fullReport.Replace(Config.User, "C:\\Users\\admin")}"
            });

            win.Show($"Created issue: {issueNew.Id}");
        }

        private static string FormatItems(string name, string returnFormat, params Item[] pairs)
        {
            if (returnFormat == "Markdown")
            {
                string str = $"## {name}\n\n```yml";

                foreach (var param in pairs)
                    str = $"{str}\n{param.Key}: {param.Value}";

                return $"{str}\n```\n\n";
            }
            else
            {
                string str = $"<h3>{name}</h3>\n<code>";

                foreach (var param in pairs)
                    str = $"{str}\n{param.Key}: {param.Value}<br>";

                return $"{str}\n</code>";
            }
        }

        public static string GetErrorData(this Config conf, string returnFormat = "Markdown")
        {
            string strData = "";

            strData = strData + FormatItems("Config Info", returnFormat,
                new Item(nameof(conf.ControllerApi), conf.ControllerApi),
                new Item(nameof(conf.IsNX), conf.IsNX),
                new Item(nameof(conf.ModPack), conf.ModPack)
            );

            strData = strData + FormatItems("Directory Info", returnFormat,
                new Item(nameof(conf.Dirs.Dynamic), conf.Dirs.Dynamic),
                new Item(nameof(conf.Dirs.Base), conf.Dirs.Base),
                new Item(nameof(conf.Dirs.Update), conf.Dirs.Update),
                new Item(nameof(conf.Dirs.DLC), conf.Dirs.DLC),
                new Item(nameof(conf.Dirs.BCML), conf.Dirs.BCML),
                new Item(nameof(conf.Dirs.MLC01), conf.Dirs.MLC01),
                new Item(nameof(conf.Dirs.Python), conf.Dirs.Python)
            );

            strData = strData + FormatItems("Install Info", returnFormat,
                new Item(nameof(conf.Install.Cemu), conf.Install.Cemu),
                new Item(nameof(conf.Install.Base), conf.Install.Base),
                new Item(nameof(conf.Install.Update), conf.Install.Update),
                new Item(nameof(conf.Install.DLC), conf.Install.DLC),
                new Item(nameof(conf.Install.Python), conf.Install.Python)
            );

            return strData;
        }

        public static string FormatReportAsMarkdown(ExceptionViewModel ex, Config conf, string log)
        {
            return new(
                $"# {ex.Title}\n\n" +
                $"> {ex.Message}\n\n" +
                $"```\n" +
                $"{ex.ExtendedMessageStr}\n" +
                $"```\n\n<br>\n" +
                $"## Install Log\n\n" +
                $"```\n" +
                $"{log}\n" +
                $"```\n\n" +
                $"" +
                $"{GetErrorData(conf)}"
            );
        }

        public static string FormatReportAsHtml(ExceptionViewModel ex, Config conf, string log)
        {
            #region HTML Meta-data/CSS

            string css = new(
                "<style>" +
                "html {" +
                "    background: #121212;" +
                "    color: #e1e1e1;" +
                "    padding: 15px;" +
                "}" +
                "hr {" +
                "    border-color: #7160E8;" +
                "    margin-left: 30px;" +
                "    margin-right: 30px;" +
                "}" +
                "h1 {" +
                "    padding-top: 15px;" +
                "    padding-bottom: 0px;" +
                "    font-size: 40px;" +
                "    padding-left: 30px;" +
                "    margin-bottom: 0px;" +
                "}" +
                "#contact {" +
                "    padding-top: 15px;" +
                "    padding-bottom: 0px;" +
                "    font-size: 20px;" +
                "    padding-left: 30px;" +
                "}" +
                "h3 {" +
                "    padding-left: 50px;" +
                "}" +
                "p {" +
                "    padding-left: 60px;" +
                "    font-size: 26px;" +
                "    font-family: \"Calibri\";" +
                "    font-weight: lighter;" +
                "}" +
                "span {" +
                "    font-style: italic;" +
                "    color: #797979;" +
                "}" +
                "code {" +
                "display: inline-block;" +
                "    background: #353535;" +
                "    margin-left: 60px;" +
                "    padding: 5px;" +
                "    padding-left: 10px;" +
                "    padding-right: 10px;" +
                "    border-radius: 5px;" +
                "    line-height: 25px;" +
                "    font-family: 'Red Hat Mono', monospace;;" +
                "    font-size: 14px;" +
                "    font-weight: bold;" +
                "}" +
                "</style>"
            );

            string htmlHeader = new(
                $"<!DOCTYPE html>" +
                $"<html lang=\"en\">" +
                $"<head>" +
                $"\t<meta charset=\"UTF-8\">" +
                $"\t<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">" +
                $"\t<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">" +
                $"\t<title>{ex.Title}</title>{css}" +
                $"</head>" +
                $"<body>"
            );

            string htmlFooter = new(
                $"</body>" +
                $"</html>"
            );

            #endregion

            string body = new(
                $"<span>*The contents of this page will be uploaded to a public or private GitHub repository</span><br>" +
                $"<h1>{ex.Title}</h1>" +
                $"<label id=\"contact\">{ex.ContactInfo}<label><hr>" +
                $"<p>{ex.Message}</p>" +
                $"<code>{ex.ExtendedMessageStr}</code><br><br>" +
                $"<h3>Install Log</h3>" +
                $"<code>{log}</code><br>" +
                $"{GetErrorData(conf, "HTML")}"
            );

            return new(
                $"{htmlHeader}{body}{htmlFooter}"
            );
        }
    }
}
