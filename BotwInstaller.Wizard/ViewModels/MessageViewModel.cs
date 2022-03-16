#pragma warning disable CS0108
#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8612
#pragma warning disable CS8618

using BotwInstaller.Wizard.Helpers;
using BotwInstaller.Wizard.ViewResources.Data;
using MaterialDesignThemes.Wpf;
using Stylet;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace BotwInstaller.Wizard.ViewModels
{
    public class MessageViewModel : Screen, INotifyPropertyChanged
    {
        #region Actions

        public void Yes()
        {
            RequestClose(false);
        }

        public void No()
        {
            RequestClose(true);
        }

        public void Copy()
        {
            RequestClose(true);
        }

        #endregion

        #region Props

        private bool _isYesNo = false;
        public bool IsYesNo
        {
            get { return _isYesNo; }
            set
            {
                _isYesNo = value;

                if (_isYesNo)
                {
                    OkMode = "No";
                    YesBtnVisibility = Visibility.Visible;
                }
                else
                {
                    OkMode = "Ok";
                    YesBtnVisibility = Visibility.Collapsed;
                }

                NotifyPropertyChanged();
            }
        }

        private string _title = "Notice";
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                NotifyPropertyChanged();
            }
        }

        private TextBlock _message = new() { Text = "~No details were provided.~" };
        public TextBlock Message
        {
            get { return _message; }
            set
            {
                _message = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Bindings

        private static ITheme GetTheme()
        {
            PaletteHelper helper = new();
            return helper.GetTheme();
        }

        private Brush _foreground = new SolidColorBrush(GetTheme().Body);
        public Brush Foreground
        {
            get { return _foreground; }
            set
            {
                _foreground = value;
                NotifyPropertyChanged();
            }
        }

        private string _okMode = "Ok";
        public string OkMode
        {
            get { return _okMode; }
            set
            {
                _okMode = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility _btnVisibility = Visibility.Hidden;
        public Visibility YesBtnVisibility
        {
            get { return _btnVisibility; }
            set
            {
                _btnVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private double _width = 220;
        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        public MessageViewModel(string message, string title = "Notice", bool isYesNo = false, string? messageColor = null, double width = 220, bool render = true)
        {
            Message = message.RenderMarkdown();
            Title = title;
            IsYesNo = isYesNo;
            Width = width;

            if (messageColor != null)
                Foreground = (Brush)new BrushConverter().ConvertFromString(messageColor);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
