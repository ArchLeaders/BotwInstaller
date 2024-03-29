﻿using Stylet;
using System.Threading.Tasks;
using BotwInstaller.Wizard.Helpers;
using System.Windows.Controls;

namespace BotwInstaller.Wizard.ViewModels
{
    public class ExceptionViewModel : Screen
    {
        public async Task Report()
        {
            if (ShellViewModel != null)
                await GitIssue.ReportAsMarkdown(ShellViewModel);
        }

        public async Task Copy()
        {
            if (ShellViewModel != null)
                await GitIssue.ViewReportAsHtml(ShellViewModel);
        }

        private string _title = "Exception Thrown";
        public string Title
        {
            get => _title;
            set => SetAndNotify(ref _title, value);
        }

        private string _message = "Invalid Exception";
        public string Message
        {
            get => _message;
            set => SetAndNotify(ref _message, value);
        }

        private TextBlock _extendedMessage = new() { Text = "No details were provided." };
        public string ExtendedMessageStr { get; set; } = "";
        public TextBlock ExtendedMessage
        {
            get => _extendedMessage;
            set => SetAndNotify(ref _extendedMessage, value);
        }

        private string _contactInfo = "Anonymous";
        public string ContactInfo
        {
            get => _contactInfo;
            set => SetAndNotify(ref _contactInfo, value);
        }

        private bool _isReportable = true;
        public bool IsReportable
        {
            get => _isReportable;
            set => SetAndNotify(ref _isReportable, value);
        }

        public ShellViewModel? ShellViewModel = null;
    }
}
