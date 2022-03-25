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
