using Avalon.Controls;
using Avalon.Common.Interfaces;
using Avalon.Common.Settings;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using Avalon.Extensions;
using Avalon.Common.Models;

namespace Avalon
{

    /// <summary>
    /// WPF Implementation of the IConveyor for UI interactions and specific platform implementations.
    /// </summary>
    public class Conveyor : IConveyor
    {

        /// <summary>
        /// Gets a variable from the settings (TODO: character)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetVariable(string key)
        {
            var variable = App.Settings.ProfileSettings.Variables.FirstOrDefault(x => x.Key.ToLower() == key.ToLower());

            if (variable != null)
            {
                return variable.Value;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Sets a variable in the settings (TODO: character)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetVariable(string key, string value)
        {
            var variable = App.Settings.ProfileSettings.Variables.FirstOrDefault(x => x.Key.ToLower() == key.ToLower());

            if (variable != null)
            {
                variable.Value = value;
            }
            else
            {
                App.Settings.ProfileSettings.Variables.Add(new Variable(key, value));
            }

            return;
        }

        /// <summary>
        /// Replaces any variables in the provided string with the variable literal value.
        /// </summary>
        /// <param name="text"></param>
        public string ReplaceVariablesWithValue(string text)
        {
            if (text.Contains("@"))
            {
                var sb = new StringBuilder(text);

                foreach (var item in App.Settings.ProfileSettings.Variables)
                {
                    sb.Replace($"@{item.Key}", item.Value);
                }

                return sb.ToString();
            }

            return text;
        }

        /// <summary>
        /// Removes a variable from the settings (TODO: character)
        /// </summary>
        /// <param name="key"></param>
        public void RemoveVariable(string key)
        {
            for (int i = App.Settings.ProfileSettings.Variables.Count - 1; i >= 0; i--)
            {
                if (App.Settings.ProfileSettings.Variables[i].Key == key)
                {
                    App.Settings.ProfileSettings.Variables.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// The title of the main window.
        /// </summary>
        public string Title
        {
            get
            {
                string buf = "";

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    buf = App.MainWindow.Title;
                }));

                return buf;
            }
            set
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    App.MainWindow.Title = value;
                }));
            }
        }

        /// <summary>
        /// Writes output to the main terminal window.
        /// </summary>
        /// <param name="text"></param>
        public void EchoText(string text)
        {
            EchoText(text.ToLine(), TerminalTarget.Main);
        }

        /// <summary>
        /// Writes output to the specified terminal window.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="target"></param>
        public void EchoText(string text, TerminalTarget target)
        {
            EchoText(text.ToLine(), target);
        }

        /// <summary>
        /// Writes output to the specified terminal window.  This procedure writes directly to the
        /// terminal and does not go through processing as when other data comes in through the OnEcho
        /// event (that gets processed for triggers).  We don't want triggers processed from here.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="target"></param>
        public void EchoText(Line line, TerminalTarget target)
        {
            switch (target)
            {
                case TerminalTarget.None:
                case TerminalTarget.Main:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        App.MainWindow.GameTerminal.Append(line);
                    }));

                    break;
                case TerminalTarget.Communication:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        App.MainWindow.CommunicationTerminal.Append(line);
                    }));

                    break;
                case TerminalTarget.OutOfCharacterCommunication:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        App.MainWindow.OocCommunicationTerminal.Append(line);
                    }));

                    break;
            }
        }

        public void SetTickTime(int value)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                App.MainWindow.InfoBar.TickTimer = value;
            }));
        }

        public int GetTickTime()
        {
            return App.MainWindow.InfoBar.TickTimer;
        }

        public string GetGameTime()
        {
            return App.MainWindow.InfoBar.Time;
        }

        /// <summary>
        /// Clears the contents of the specified terminal.
        /// </summary>
        /// <param name="target"></param>
        public void ClearTerminal(TerminalTarget target)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                switch (target)
                {
                    case TerminalTarget.None:
                        break;
                    case TerminalTarget.Main:
                        App.MainWindow.GameTerminal.Text = "";
                        break;
                    case TerminalTarget.Communication:
                        App.MainWindow.CommunicationTerminal.Text = "";
                        break;
                    case TerminalTarget.OutOfCharacterCommunication:
                        App.MainWindow.OocCommunicationTerminal.Text = "";
                        break;
                }
            }));
        }

        /// <summary>
        /// The number of lines in a given terminal.
        /// </summary>
        /// <param name="target"></param>
        public int LineCount(TerminalTarget target)
        {
            switch (target)
            {
                case TerminalTarget.None:
                    return 0;
                case TerminalTarget.Main:
                    return App.MainWindow.GameTerminal.LineCount;
                case TerminalTarget.Communication:
                    return App.MainWindow.CommunicationTerminal.LineCount;
                case TerminalTarget.OutOfCharacterCommunication:
                    return App.MainWindow.OocCommunicationTerminal.LineCount;
            }

            return 0;
        }

        public ProfileSettings ProfileSettings => App.Settings.ProfileSettings;
    }
}
