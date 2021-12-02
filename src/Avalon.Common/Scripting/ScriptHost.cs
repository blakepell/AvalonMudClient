/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Argus.Extensions;
using Avalon.Common.Models;
using Avalon.Common.Settings;
using Cysharp.Text;

namespace Avalon.Common.Scripting
{
    /// <summary>
    /// Container to house all of the available scripting environments.
    /// </summary>
    public class ScriptHost
    {
        /// <summary>
        /// MoonSharp Lua Engine.
        /// </summary>
        public MoonSharpEngine MoonSharp { get; set; }

        /// <summary>
        /// The number of scripts that are currently active between all environments.
        /// </summary>
        public ScriptStatistics Statistics { get; }

        /// <summary>
        /// A list of current instances of any scripts so Script objects from the memory pool can pull
        /// from here when they need a new copy.  The key will be the function name.
        /// </summary>
        public Dictionary<string, SourceCode> SourceCodeIndex { get; set; } = new();

        /// <summary>
        /// Locks various functions of the ScriptHost, including the ability to add functions
        /// into the <see cref="SourceCodeIndex"/>.  Due to the fact that the index is updated
        /// based on the property changes of aliases and triggers it becomes problematic when you
        /// want to load a profile to observe it (which then loaded those assets).  This will
        /// provide the ability to pause that behavior so profiles can be loaded without creating
        /// a cross boundary issue of one profile loading scripts into another (a cautionary tale of
        /// why putting logic in properties is always a last resort).
        /// </summary>
        public bool Lock { get; set; } = false;

        /// <summary>
        /// Creates a new instance of the <see cref="ScriptHost"/> with all scripting environments
        /// initialized for use.
        /// </summary>
        public ScriptHost()
        {
            this.MoonSharp = new MoonSharpEngine(this);
            this.Statistics = new ScriptStatistics();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ScriptHost"/> with only the specified scripting
        /// environments initialized for use.
        /// </summary>
        /// <param name="enableMoonSharp"></param>
        public ScriptHost(bool enableMoonSharp)
        {
            if (enableMoonSharp)
            {
                this.MoonSharp = new MoonSharpEngine(this);
            }

            this.Statistics = new ScriptStatistics();
        }

        /// <summary>
        /// Registers an object with all of the available script engines.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="item"></param>
        /// <param name="prefix"></param>
        public void RegisterObject<T>(Type t, object item, string prefix)
        {
            MoonSharp?.RegisterObject<T>(t, item, prefix);
        }

        /// <summary>
        /// Resets all scripting engines to their default state.
        /// </summary>
        public void Reset()
        {
            MoonSharp?.Reset();
            SourceCodeIndex.Clear();
        }

        /// <summary>
        /// Saves a function into the global code index used to sync scripts across
        /// scripting environments and memory pool entries of scripting environments.
        /// </summary>
        /// <param name="src"></param>
        public void AddFunction(SourceCode src)
        {
            // ScriptHost is locked, get out.
            if (this.Lock)
            {
                return;
            }

            this.SourceCodeIndex[src.FunctionName] = src;
        }

        /// <summary>
        /// Refreshes all Lua scripts.  This will attempt to load them from the DI provided
        /// settings if it exists.
        /// </summary>
        public void RefreshScripts()
        {
            var settings = AppServices.GetService<SettingsProvider>();

            if (settings?.ProfileSettings == null)
            {
                return;
            }

            foreach (var alias in settings.ProfileSettings.AliasList)
            {
                // In case any of the function names are null or blank, set them up based off of the ID.
                if (string.IsNullOrWhiteSpace(alias.FunctionName))
                {
                    alias.FunctionName = ScriptHost.GetFunctionName(alias.Id, "a");
                }

                // Load the scripts into the scripting environment.  AddFunction updates the dictionary
                // by key so this will in effect add or update (not duplicate)
                if (alias.ExecuteAs == ExecuteType.LuaMoonsharp)
                {
                    this.AddFunction(new SourceCode(alias.Command, alias.FunctionName, ScriptType.MoonSharpLua));
                }
            }

            foreach (var trigger in settings.ProfileSettings.TriggerList)
            {
                // In case any of the function names are null or blank, set them up based off of the ID.
                if (string.IsNullOrWhiteSpace(trigger.FunctionName))
                {
                    trigger.FunctionName = ScriptHost.GetFunctionName(trigger.Identifier, "tr");
                }

                // Load the scripts into the scripting environment.
                if (trigger.ExecuteAs == ExecuteType.LuaMoonsharp)
                {
                    this.AddFunction(new SourceCode(trigger.Command, trigger.FunctionName, ScriptType.MoonSharpLua));
                }
            }
        }

        /// <summary>
        /// Returns a supported function name for the provided function name.  When wrapping functions
        /// with dynamic names a Guid might be used (and this will clean that value up so that all
        /// scripting environments should be supported by it).
        /// </summary>
        /// <param name="functionName">A function name that will remove unsupported characters.</param>
        /// <param name="prefix">Prefix to help identify where the function was loaded from in case the user
        /// has different parts of the programming creating functions.</param>
        public static string GetFunctionName(string functionName, string prefix = null)
        {
            using (var sb = ZString.CreateStringBuilder())
            {
                sb.Append("func_");

                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    sb.Append(prefix);
                    sb.Append('_');
                }

                for (int i = 0; i < functionName.Length; i++)
                {
                    if (functionName[i].IsLetter() || functionName[i].IsNumber())
                    {
                        sb.Append(functionName[i]);
                    }
                    else if (functionName[i].Equals('-') || functionName[i].Equals('_'))
                    {
                        sb.Append('_');
                    }
                }

                return sb.ToString();
            }
        }
    }
}
