using Avalon.Common.Colors;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Avalon.Lua
{
    /// <summary>
    /// C# methods that are exposed to LUA.
    /// </summary>
    public class LuaCommands
    {
        public LuaCommands(Interpreter interp)
        {
            _interpreter = interp;
        }

        /// <summary>
        /// Sends text to the server.
        /// </summary>
        /// <param name="cmd"></param>
        public async void Send(string cmd)
        {
            Application.Current.Dispatcher.Invoke(new Action(async () =>
            {
                await _interpreter.Send(cmd, false, false);
            }));
        }

        /// <summary>
        /// Gets a variable from the profile's global variable list.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetVariable(string key)
        {
            return Application.Current.Dispatcher.Invoke(new Func<string>(() => _interpreter.Conveyor.GetVariable(key))
            );
        }

        /// <summary>
        /// Sets a variable in the profile's global variable list.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetVariable(string key, string value)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _interpreter.Conveyor.SetVariable(key, value);
            }));
        }

        /// <summary>
        /// Echos text to the main terminal.
        /// </summary>
        /// <param name="msg"></param>
        public void Echo(string msg)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _interpreter.EchoText(msg, AnsiColors.Cyan);
            }));
        }

        /// <summary>
        /// Echos an event to the main terminal.
        /// </summary>
        /// <param name="msg"></param>
        public void EchoEvent(string msg)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _interpreter.EchoText(msg, AnsiColors.Cyan, true);
            }));
        }

        /// <summary>
        /// Returns the current time in HH:MM:SS format.
        /// </summary>
        /// <returns></returns>
        public string GetTime()
        {
            return $"{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}";
        }

        /// <summary>
        /// Returns the current hour.
        /// </summary>
        /// <returns></returns>
        public int GetHour()
        {
            return DateTime.Now.Hour;
        }

        /// <summary>
        /// Returns the current minute.
        /// </summary>
        /// <returns></returns>
        public int GetMinute()
        {
            return DateTime.Now.Minute;
        }

        /// <summary>
        /// Returns the current second.
        /// </summary>
        /// <returns></returns>
        public int GetSecond()
        {
            return DateTime.Now.Second;
        }

        /// <summary>
        /// Returns the current millisecond.
        /// </summary>
        /// <returns></returns>
        public int GetMillisecond()
        {
            return DateTime.Now.Millisecond;
        }

        /// <summary>
        /// Will pause the Lua script for the designated amount of milliseconds.  This is not async
        /// so it will block the Lua (but since Lua is called async the rest of the program continues
        /// to work).  This will be an incredibly useful and powerful command for those crafting Lua scripts.
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