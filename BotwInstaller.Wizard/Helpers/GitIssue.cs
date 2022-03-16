using BotwInstaller.Lib;
using BotwInstaller.Wizard.Helpers;
using BotwInstaller.Wizard.ViewModels;
using BotwInstaller.Wizard.ViewResources;
using BotwInstaller.Wizard.ViewResources.Data;
using Octokit;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BotwInstaller.Wizard.Helpers
{
    public static class GitIssue
    {
        public static void Copy(this ShellViewModel shell)
        {
            // Clipboard.SetText(shell.FormattedError.Replace(Config.User, "C:\\Users\\admin"));
        }

        public static async Task Report(this ShellViewModel shell)
        {
            // Using old system with shell
            // update before release

            IWindowManager win = shell.WindowManager;

            if (!win.Show($"{ToolTips.ReportError}\n\nContinue anyway?", "Privacy Warning", true, width: 500))
                return;

            bool isPublic = win.Show("Would you like to upload to the public GitHub repository?\n\n" +
                "https://github.com/ArchLeaders/BotwInstaller", "", true, width: 300);

            string repo = isPublic ? "botwinstaller" : "botwinstaller-issues";

            // shell.IsReportable = false;

            // Create client
            var client = new GitHubClient(new ProductHeaderValue("botw-installer-v3"));
            client.Credentials = new Credentials(AuthKey.Get);

            // Get issues
            var issues = await client.Issue.GetAllForRepository("archleaders", repo);

            // Update issue if it exists
            foreach (var issue in issues)
            {
                if (issue.Title == "") //shell.Exception
                {
                    IssueUpdate issueUpdate = new();
                    // issueUpdate.Body = $"{issue.Body}\n\n---\n\n{shell.FormattedError.Replace(Config.User, "C:\\Users\\admin")}";

                    // await client.Issue.Update("archleaders", repo, issue.Number, issueUpdate);
                    // win.Show($"Updated issue: {issue.Id}");
                    return;
                }
            }

            // Create new issue
            var issueNew = await client.Issue.Create("archleaders", repo, new NewIssue("") // shell.Exception
            {
                // Body = shell.FormattedError.Replace(Config.User, "C:\\Users\\admin")
            });

            win.Show($"Created issue: {issueNew.Id}");
        }
    }
}
