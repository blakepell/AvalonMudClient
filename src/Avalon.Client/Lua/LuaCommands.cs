using Avalon.Common.Colors;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;

namespace Avalon.Lua
{
    /// <summary>
    /// Commands that are exposed to LUA.
    /// </summary>
    public class LuaCommands
    {
        public LuaCommands(Interpreter interp)
        {
            _interpreter = interp;
        }

        public async void Send(string cmd)
        {
            Application.Current.Dispatcher.Invoke(new Action(async () =>
            {
                await _interpreter.Send(cmd, false, false);
            }));
        }

        public string GetVariable(string key)
        {
            return Application.Current.Dispatcher.Invoke(new Func<string>(() => _interpreter.Conveyor.GetVariable(key))
            );
        }

        public void SetVariable(string key, string value)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _interpreter.Conveyor.SetVariable(key, value);
            }));
        }

        public void Echo(string msg)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _interpreter.EchoText(msg, AnsiColors.Cyan);
            }));
        }

        public void EchoEvent(string msg)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _interpreter.EchoText(msg, AnsiColors.Cyan, true);
            }));
        }

        public string GetTime()
        {
            return $"{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}";
        }

        public int GetHour()
        {
            return DateTime.Now.Hour;
        }

        public int GetMinute()
        {
            return DateTime.Now.Minute;
        }

        public int GetSecond()
        {
            return DateTime.Now.Second;
        }

        public int GetMillesecond()
        {
            return DateTime.Now.Millisecond;
        }

        /// <summary>
        /// Will pause the Lua script for the designated amount of milliseconds.  This is not async
        /// so it will block the Lua (but since Lua is called async the rest of the program continues
        /// to work).
        /// </summary>
        /// <param name="milliseconds"></param>
        public void Sleep(int milliseconds)
        {
            Task.Delay(milliseconds).Wait();
        }

        /// <summary>
        /// Returns a random number.
        /// </summary>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public int RandomNumber(int low, int high)
        {
            var rnd = new Random();
            return rnd.Next(low, high);
        }

        /// <summary>
        /// Returns a new GUID.
        /// </summary>
        /// <returns></returns>
        public string Guid()
        {
            return System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Sets the mud client's title.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public void SetTitle(string title)
        {
            _interpreter.Conveyor.Title = title;
        }

        private Interpreter _interpreter;

    }
}