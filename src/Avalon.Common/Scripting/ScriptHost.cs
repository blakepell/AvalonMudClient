/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Argus.Extensions;
using Cysharp.Text;
using System;

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

        ///// <summary>
        ///// NLua Lua Engine.
        ///// </summary>
        //public NLuaEngine NLua { get; set; }

        /// <summary>
        /// The number of scripts that are currently active between all environments.
        /// </summary>
        public ScriptStatistics Statistics { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ScriptHost"/> with all scripting environments
        /// initialized for use.
        /// </summary>
        public ScriptHost()
        {
            this.MoonSharp = new MoonSharpEngine(this);
            //this.NLua = new NLuaEngine(this);
            this.Statistics = new ScriptStatistics();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ScriptHost"/> with only the specified scripting
        /// environments initialized for use.
        /// </summary>
        /// <param name="enableMoonSharp"></param>
        /// <param name="enableNLua"></param>
        public ScriptHost(bool enableMoonSharp, bool enableNLua)
        {
            if (enableMoonSharp)
            {
                this.MoonSharp = new MoonSharpEngine(this);
            }

            //if (enableNLua)
            //{
            //    this.NLua = new NLuaEngine(this);
            //}

            this.Statistics = new ScriptStatistics();
        }

        /// <summary>
        /// Registers an object with all of the available script engines.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="prefix"></param>
        public void RegisterObject<T>(Type t, object item, string prefix)
        {
            MoonSharp?.RegisterObject<T>(t, item, prefix);
            //NLua?.RegisterObject<T>(t, item, prefix);
        }

        /// <summary>
        /// Resets all scripting engines to their default state.
        /// </summary>
        public void Reset()
        {
            MoonSharp?.Reset();
            //NLua?.Reset();
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
