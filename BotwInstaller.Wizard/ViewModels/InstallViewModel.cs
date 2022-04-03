using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BotwInstaller.Wizard.ViewModels
{
    public class UpdateID
    {
        public double Value { get; set; } = 0;
        public int FractionMax { get; set; } = 0;
        public int FractionCurrent { get; set; } = 0;
        public int Pos { get; set; } = 0;
        public DispatcherTimer Updater { get; set; } = new();

        public UpdateID(InstallViewModel vM, int pos, int interval = 80, bool isFractional = false, bool isLoop = false)
        {
            Pos = pos;

            Updater.Interval = new TimeSpan(0, 0, 0, 0, interval);
            Updater.Tick += (s, e) =>
            {
                if (Value > vM.BoundValues[Pos])
                {
                    vM.BoundValues[Pos] += 1;

                    if (isFractional)
                        vM.BoundStrValues[Pos] = $"{FractionCurrent}/{FractionMax}";
                    else
                        vM.BoundStrValues[Pos] = $"{Math.Round(vM.BoundValues[Pos])}%";

                    if (isLoop)
                    {
                        if (vM.BoundValues[Pos] == 100)
                        {
                            vM.BoundValues[Pos] = 0;
                            Value = 0;
                        }
                    }
                }
            };

            Updater.Start();
        }
    }

    public class InstallViewModel : Screen
    {
        private static Dictionary<string, string> TitleKeys = new()
        {
            { "Installing .", "Installing . ." },
            { "Installing . .", "Installing . . ." },
            { "Installing . . .", "Installing ." },
        };


        private ObservableCollection<double> _boundValues = new();
        public ObservableCollection<double> BoundValues
        {
            get => _boundValues;
            set => SetAndNotify(ref _boundValues, value);
        }

        private ObservableCollection<string> _boundStrValues = new();
        public ObservableCollection<string> BoundStrValues
        {
            get => _boundStrValues;
            set => SetAndNotify(ref _boundStrValues, value);
        }

        public List<DispatcherTimer> Timers { get; set; } = new();
        public Dictionary<string, UpdateID> UnboundValues { get; set; } = new();

        public void LogMessage(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            Log = $"{Log}\n{text}";
            ScrollUpdater = !ScrollUpdater;
        }

        public void Update(double value, string id)
        {
            if (id.EndsWith('%'))
            {
                UnboundValues[id.Replace("%", "")].Updater.Interval = new TimeSpan(0, 0, 0, 0, (int)Math.Round(value));
            }
            else if (id.EndsWith('+'))
            {
                UnboundValues[id.Replace("+", "")].Value = UnboundValues[id.Replace("+", "")].Value + value;
            }
            else if (id.EndsWith('@'))
            {
                UnboundValues[id.Replace("@", "")].FractionMax = (int)(UnboundValues[id.Replace("@", "")].Value + value);
            }
            else
            {
                UnboundValues[id].Value = value;
            }
        }

        public void ScrollViewerSizeChanged(ScrollViewer sender, DependencyPropertyChangedEventArgs e)
        {
            sender.ScrollToBottom();
        }

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

        public InstallViewModel()
        {
            BoundValues.Clear();
            BoundStrValues.Clear();
            Timers.Clear();
            UnboundValues.Clear();

            UnboundValues.Add("bcml", new(this, 0));
            UnboundValues.Add("cemu", new(this, 1));
            UnboundValues.Add("game", new(this, 2, 120));
            UnboundValues.Add("tool", new(this, 3, isLoop: true));

            for (int i = 0; i < 4; i++)
            {
                BoundValues.Add(0);
                BoundStrValues.Add("0%");
            }

            DispatcherTimer timer = new();
            timer.Interval = new TimeSpan(0, 0, 0, 1);
            timer.Tick += (s, e) => Title = TitleKeys[Title];
            timer.Start();
        }
    }
}