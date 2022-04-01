namespace BotwInstaller.Wizard.ViewResources.Data
{
    public static class ToolTips
    {
        public static string CopyBaseGameFiles
        {
            get => "Copies the base game files into Cemu's mlc01 directory.\n" +
                "(Recomended if your files are on an SDCard)";
        }

        public static string ReportError
        {
            get => "Reporting an error will upload system information such as file paths to a private or public GitHub Repository.\n" +
                "Your username is hidden; however, any other names or information in file/folder paths will be uploaded.\n" +
                "Please regard this before proceeding.\n\n" +
                "Auto reporting will expire on 12/31/2022 or sooner to maintain security.";
        }
    }
}
