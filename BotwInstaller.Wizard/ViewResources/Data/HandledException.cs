using System;

namespace BotwInstaller.Wizard.ViewResources.Data
{
    public class HandledException
    {
        public string Exception { get; set; } = "Exception Thrown";
        public string Message { get; set; } = "";
        public string? ExtendedMessage { get; set; } = null;
    }
}