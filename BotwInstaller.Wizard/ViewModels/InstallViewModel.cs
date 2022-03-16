using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BotwInstaller.Wizard.ViewModels
{
    public class InstallViewModel : Screen
    {
        private static Dictionary<string, string> TitleKeys = new()
        {
            { "Installing .", "Installing . ." },
            { "Installing . .", "Installing . . ." },
            { "Installing . . .", "Installing ." },
        };

        public Dictionary<string, double> UValues = new()
        {
            { "game", 0.0 },
            { "cemu", 0.0 },
            { "bcml", 0.0 }
        };
        
        public void LogMessage(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            Log = $"{Log}\n{text}";
            ScrollUpdater = !ScrollUpdater;
        }

        public void Update(double value, string id)
        {

        }

        public void ScrollViewerSizeChanged(ScrollViewer sender, DependencyPropertyChangedEventArgs e)
        {
            sender.ScrollToBottom();
        }

        public DispatcherTimer UpdateTimer { get; } = new();

        private bool _scrollUpdater = true;
        public bool ScrollUpdater
        {
            get => _scrollUpdater;
            set => SetAndNotify(ref _scrollUpdater, value);
        }

        private string _title = "Installing .";
        public string Title
        {
            get => _title;
            set => SetAndNotify(ref _title, value);
        }

        private string _log = "Loading System Log . . .";
        public string Log
        {
            get => _log;
            set => SetAndNotify(ref _log, value);
        }

        #region String Values

        private string _strGameValue = "0%";
        public string StrGameValue
        {
            get => _strGameValue;
            set => SetAndNotify(ref _strGameValue, value);
        }

        private string _strCemuValue = "0%";
        public string StrCemuValue
        {
            get => _strCemuValue;
            set => SetAndNotify(ref _strCemuValue, value);
        }

        private string _strBcmlValue = "0%";
        public string StrBcmlValue
        {
            get => _strBcmlValue;
            set => SetAndNotify(ref _strBcmlValue, value);
        }

        #endregion

        #region Double Values

        private double _gameValue;
        public double GameValue
        {
            get => _gameValue;
            set => SetAndNotify(ref _gameValue, value);
        }

        private double _cemuValue;
        public double CemuValue
        {
            get => _cemuValue;
            set => SetAndNotify(ref _cemuValue, value);
        }

        private double _bcmlValue;
        public double BcmlValue
        {
            get => _bcmlValue;
            set => SetAndNotify(ref _bcmlValue, value);
        }

        #endregion

        public InstallViewModel()
        {
            DispatcherTimer timer = new();
            timer.Interval = new TimeSpan(0, 0, 0, 1);
            timer.Tick += (s, e) => Title = TitleKeys[Title];
            timer.Start();
        }
    }
}