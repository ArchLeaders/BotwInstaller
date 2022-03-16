using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotwInstaller.Wizard.ViewModels
{
    public class ExceptionViewModel : Screen
    {

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

        private string _extendedMessage = "No details were provided.";
        public string ExtendedMessage
        {
            get => _extendedMessage;
            set => SetAndNotify(ref _extendedMessage, value);
        }

        private bool _isReportable = true;
        public bool IsReportable
        {
            get => _isReportable;
            set => SetAndNotify(ref _isReportable, value);
        }
    }
}
