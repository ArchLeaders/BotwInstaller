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

        private static string FormatItems(string name, string returnFormat, params Item[] pairs)
        {
            if (returnFormat == "Markdown")
            {
                string str = $"## {name}\n\n```yml";

                foreach (var param in pairs)
                    str = $"{str}\n{param.Key}: {param.Value}";

                return $"{str}\n```";
            }
            else if (returnFormat == "CSS")
            {
                return "";
            }
            else
            {
                string str = $"<h3>{name}</h3>\n<code>";

                foreach (var param in pairs)
                    str = $"{str}\n{param.Key}: {param.Value}";

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

        public static TextBlock RenderMarkdown(this string text)
        {
            TextBlock textBlock = new() { TextWrapping = TextWrapping.WrapWithOverflow };

            string span = "";
            string hrefSpan = "";
            string href = "";

            bool inHrefSpan = false;
            bool isEndHrefSpan = false;
            bool inHref = false;
            bool isEscaped = false;

            bool bold = false;
            bool italic = false;
            bool underline = false;

            foreach (char _char in text)
            {
                // Escape

                if (_char == '\\')
                {
                    isEscaped = true;
                }

                else if (isEscaped)
                {
                    span = $"{span}{_char}";
                    isEscaped = false;
                }

                // Bold
                else if (_char == '*')
                {
                    UpdateBool(ref bold);
                }

                // Italic
                else if (_char == '~')
                {
                    UpdateBool(ref italic);
                }

                // Underline
                else if (_char == '_')
                {
                    UpdateBool(ref underline);
                }

                // Hyperlink

                else if (_char == '[' && !inHrefSpan)
                {
                    textBlock.Inlines.Add(GetRun(span));
                    span = "";
                    inHrefSpan = true;
                }

                else if (_char == ']' && inHrefSpan)
                {
                    inHrefSpan = false;
                    isEndHrefSpan = true;
                }

                else if (_char == '(' && isEndHrefSpan)
                {
                    inHref = true;
                    isEndHrefSpan = false;
                }

                else if (_char != '(' && isEndHrefSpan)
                {
                    isEndHrefSpan = false;
                    textBlock.Inlines.Add($"[{hrefSpan}]{_char}");
                    hrefSpan = "";
                }

                else if (_char == ')' && inHref)
                {
                    inHref = false;

                    Hyperlink hyperlink = new() { NavigateUri = new(href) };
                    hyperlink.RequestNavigate += Actions.OpenUrl;

                    hyperlink.Inlines.Add(GetRun(hrefSpan));
                    textBlock.Inlines.Add(hyperlink);

                    hrefSpan = ""; href = "";
                }

                else if (inHrefSpan)
                {
                    hrefSpan = $"{hrefSpan}{_char}";
                }

                else if (inHref)
                {
                    href = $"{href}{_char}";
                }

                else
                {
                    span = $"{span}{_char}";
                }
            }

            textBlock.Inlines.Add(GetRun(span));
            textBlock.Inlines.Add(GetRun(hrefSpan));

            return textBlock;

            void UpdateBool(ref bool value)
            {
                textBlock.Inlines.Add(GetRun(span));
                value = !value;
                span = "";
            }

            Run GetRun(string text)
            {
                Run run = new(text)
                {
                    FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
                    FontStyle = italic ? FontStyles.Italic : FontStyles.Normal
                };

                if (underline)
                    run.TextDecorations.Add(TextDecorations.Underline);

                return run;
            }
        }

        public static string MarkdownFormat(string title, HandledException ex, Config conf)
        {
            return new(
                $"# {title}\n\n" +
                $"> {ex.Message}\n\n" +
                $"```\n" +
                $"{ex.ExtendedMessage}\n" +
                $"```\n\n" +
                $"{GetErrorData(conf)}"
            );
        }
        
        public static string HtmlFormat(string title, HandledException ex, Config conf)
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
                $"\t<title>{title}</title>{css}" +
                $"</head>" +
                $"<body>"
            );

            string htmlFooter = new(
                $"</body>" +
                $"</html>"
            );

            #endregion

            string body = new(
                $"<span>*The contents of this page will be uploaded to a public or private GitHub repository</span>" +
                $"<h1>{title}</h1>" +
                $"<hr>" +
                $"<p>{ex.Message}</p>{GetErrorData(conf, "HTML")}"
            );

            return new(
                $"{htmlHeader}{css}{body}{htmlFooter}"
            );
        }

        public static Dictionary<string, HandledException> HandledExceptions { get; set; } = new()
        {
            {
                "Network Error", new()
                {
                    Exception = "The request was canceled due to the configured HttpClient.Timeout of ",
                    Message = "The server took too long to respond.",
                    ExtendedMessage = "If the issue persists report the error.\n\n" +
                        "[Error Details]\n$exmsg\n$exstack"
                }
            },
        };
    }
}
